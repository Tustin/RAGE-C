using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RAGE
{
    public static class Parser
    {
        public static List<string> GenerateEntryPoint(bool networkScript = false)
        {
            List<string> code = new List<string>
            {
                ":__script__entry",
                "Function 0 2 0"
            };
            if (networkScript)
            {
                code.Add("CallNative \"NETWORK_SET_SCRIPT_IS_SAFE_FOR_NETWORK_GAME\" 0 0");
            }
            code.Add("Call @main");
            code.Add("Return 0 0");
            code.Add("");
            return code;
        }

        public static Function GetFunctionContents(string functionName)
        {
            Function function = new Function();

            bool foundFunction = false;
            bool inFunctionBlock = false;
            bool inLogicBlock = false;
            bool inLoopBlock = false;

            int conditionalCount = 0;
            int loopCount = 0;

            foreach (string code in Core.RawScriptCode)
            {
                string line = code.Trim();

                List<string> pieces = line.ExplodeAndClean(' ');

                if (!line.Contains(functionName) && !foundFunction)
                {
                    continue;
                }

                else if (!foundFunction && line.IsFunction())
                {
                    FunctionDeclaration info = line.GetFunctionInfo();
                    function.ReturnType = info.ReturnType;
                    function.Name = info.Name;
                    function.Arguments = info.Arguments;
                    foundFunction = true;
                }

                //we found the function and this list contains an open curly bracket and we currently aren't in a block
                //we can assume this is the start
                if (foundFunction && line.Contains('{') && !inFunctionBlock)
                {
                    inFunctionBlock = true;
                    continue;
                }

                //stuff for C keywords
                bool foundKeyword = Keyword.IsMatch(pieces[0], out string keyword);

                //we found the function, are currently in it but we found an opening bracket
                //we can assume this is an if logic block
                if (foundFunction && foundKeyword && keyword == "if" && inFunctionBlock)
                {
                    List<string> elements = line.ExplodeAndClean(' ');

                    //makes it so you can either put the { on the same line as the if or on the line after
                    int startPosition = function.Code.Count + 1;

                    if (!line.Contains("{"))
                    {
                        startPosition = function.Code.Count + 2;
                    }

                    Conditional conditional = new Conditional(ConditionalTypes.If, startPosition, null)
                    {
                        Index = conditionalCount++,
                        Function = function
                    };

                    //lets see if this conditional is nested
                    if (function.Conditionals.AreThereAnyParentConditionals(conditional) && function.AreThereAnyUnclosedBlocks<Conditional>())
                    {
                        Conditional lastConditional = function.Conditionals.GetLastConditional(conditional);

                        //since the last one isnt closed, we can assume its the parent of this one
                        if (lastConditional.CodeEndLine == null)
                        {
                            conditional.Parent = lastConditional;
                        }
                        else
                        {
                            Conditional lastParent = function.Conditionals.GetLastUnclosedParentConditional();
                            conditional.Parent = lastParent ?? throw new Exception("Tried getting a parent for nested conditional, but failed");
                        }
                    }

                    conditional.Logic = ConditionalLogic.Parse(line);

                    function.Conditionals.Add(conditional);
                    //inLogicBlock = true;
                    function.Code.Add(line);
                    continue;
                }
                //we found the function, are currently in it but we found an opening bracket
                //we can assume this is a control loop block
                if (foundFunction && foundKeyword && (keyword == "for" || keyword == "while") && inFunctionBlock)
                {
                    ControlLoop loop = new ControlLoop()
                    {
                        Function = function
                    };
                    int startPosition = function.Code.Count + 1;
                    if (!line.Contains("{"))
                    {
                        startPosition = function.Code.Count + 2;
                    }
                    loop.CodeStartLine = startPosition;
                    loop.CodeEndLine = null;
                    loop.Index = loopCount++;

                    //if this is a nested loop, get the parent loop
                    if (function.Loops.AreThereAnyParentLoops(loop) && function.AreThereAnyUnclosedBlocks<ControlLoop>())
                    {
                        ControlLoop lastLoop = function.Loops.GetLastLoop(loop);
                        //since the last one isnt closed, we can assume its the parent of this one
                        if (lastLoop.CodeEndLine == null)
                        {
                            loop.LoopParent = lastLoop;
                        }
                        else
                        {
                            ControlLoop lastParent = function.GetLastParent<ControlLoop>();
                            loop.LoopParent = lastParent;
                        }
                    }
                    //do the same as above but if the loop is in a conditional block
                    if (function.AreThereAnyUnclosedBlocks<Conditional>())
                    {
                        Conditional lastConditional = function.GetLast<Conditional>();

                        //since the last one isnt closed, we can assume its the parent of this one
                        if (lastConditional.CodeEndLine == null)
                        {
                            loop.ConditionalParent = lastConditional;
                        }
                        else
                        {
                            Conditional lastParent = function.GetLastParent<Conditional>();
                            loop.ConditionalParent = lastParent;
                        }
                    }
                    loop.Type = ControlLoop.GetType(line);
                    loop.Logic = loop.ParseLogic(line);
                    switch (loop.Type)
                    {
                        case ControlLoopTypes.For:
                            ForLogic logic = (ForLogic)loop.Logic;
                            function.LocalVariables.Add(new Variable("int", logic.IteratorVariable, logic.InitialIteratorValue.ToString(), function.frameCount++));
                            break;
                    }

                    function.Loops.Add(loop);
                    function.Code.Add(line);
                    //inLoopBlock = true;
                    continue;
                }

                //we can assume this is an ending to a logic block        
                if (foundFunction && line.Contains('}') && inFunctionBlock
                    && function.AreThereAnyUnclosedBlocks<Conditional>()
                    && !function.AreThereAnyUnclosedBlocks<ControlLoop>())
                {
                    if (function.Conditionals.Count == 0)
                    {
                        throw new Exception("No conditionals have been found prior, how did you get this?");
                    }
                    int lastUnclosedBlock = function.GetIndexOfLastUnclosedBlock<Conditional>();
                    function.Conditionals[lastUnclosedBlock].CodeEndLine = function.Code.Count - 1;
                    inLogicBlock = false;
                    function.Code.Add(line);
                    continue;
                }

                //we can assume this is an ending to a loop block        
                if (foundFunction && line.Contains('}') && inFunctionBlock
                    && function.AreThereAnyUnclosedBlocks<ControlLoop>()
                    && !function.AreThereAnyUnclosedBlocks<Conditional>())
                {
                    if (function.Loops.Count == 0)
                    {
                        throw new Exception("No loops have been found prior, how did you get this?");
                    }
                    int lastUnclosedBlock = function.GetIndexOfLastUnclosedBlock<ControlLoop>();
                    function.Loops[lastUnclosedBlock].CodeEndLine = function.Code.Count - 1;
                    inLoopBlock = false;
                    function.Code.Add(line);
                    continue;
                }

                //if theres an unclosed if inside an unclosed loop or vice-versa
                if (foundFunction && line.Contains('}') && inFunctionBlock
                    && function.AreThereAnyUnclosedBlocks<ControlLoop>()
                    && function.AreThereAnyUnclosedBlocks<Conditional>())
                {
                    //lets do it dirty for now
                    int lastUnclosedLogicIndex = function.GetIndexOfLastUnclosedBlock<Conditional>();
                    Conditional lastUnclosedConditional = function.Conditionals[lastUnclosedLogicIndex];
                    int lastUnclosedLoopIndex = function.GetIndexOfLastUnclosedBlock<ControlLoop>();
                    ControlLoop lastUnclosedLoop = function.Loops[lastUnclosedLoopIndex];

                    int closestUnclosedBlock = Math.Max(lastUnclosedConditional.CodeStartLine, lastUnclosedLoop.CodeStartLine);
                    if (closestUnclosedBlock == lastUnclosedConditional.CodeStartLine)
                    {
                        function.Conditionals[lastUnclosedLogicIndex].CodeEndLine = function.Code.Count - 1;
                        inLogicBlock = false;
                    }
                    else if (closestUnclosedBlock == lastUnclosedLoop.CodeStartLine)
                    {
                        function.Loops[lastUnclosedLoopIndex].CodeEndLine = function.Code.Count - 1;
                        inLoopBlock = false;
                    }
                    else
                    {
                        throw new Exception("Unable to find last unclosed block");
                    }

                    function.Code.Add(line);
                    continue;
                }

                //we found the function, we're in the function code block and it contains an assignment to a new variable
                Regex newVarRegex = new Regex(@"(\w+) (\w+)\s?=\s?(.+)\;");
                if (foundFunction && inFunctionBlock && newVarRegex.IsMatch(line))
                {
                    List<string> matches = Utilities.GetRegexGroups(newVarRegex.Matches(line));
                    if (!Core.IsTypeSupported(matches[0]))
                    {
                        throw new Exception("Type hasnt been defined yet");
                    }
                    function.LocalVariables.Add(new Variable()
                    {
                        Value = matches[1],
                        ValueType = matches[0],
                        FrameId = function.frameCount++,
                        VariableValue = matches[2],
                    });
                }

                //we found the function, we're in the function code block and its not the end of the function
                if (foundFunction && inFunctionBlock && !line.Contains('}'))
                {
                    function.Code.Add(line);
                    continue;
                }

                //we found the function, we're in the function code block and we're not in a logic block but we found a closing curling bracket - we can assume its the end of the function
                if (foundFunction && inFunctionBlock && line.Contains('}') && !inLogicBlock && !inLoopBlock)
                {
                    inFunctionBlock = false;
                    return function;
                }
            }
            return null;
        }

        public static List<Function> GetAllFunctions()
        {
            List<Function> scriptFunctions = new List<Function>();
            foreach (string line in Core.RawScriptCode)
            {
                if (line.IsFunction())
                {
                    FunctionDeclaration functionInfo = line.GetFunctionInfo();
                    if (!Core.IsTypeSupported(functionInfo.ReturnType))
                    {
                        throw new Exception($"Return type for function '{functionInfo.Name}' not supported");
                    }
                    scriptFunctions.Add(GetFunctionContents(functionInfo.Name));
                }
            }
            return scriptFunctions;
        }

        public static List<string> GenerateASMFunction(Function function)
        {
            List<string> asmCode = new List<string>
            {
                ":" + function.Name,
                $"Function 0 {function.frameCount} 0"
            };

            AssemblyFunction asmFunction = new AssemblyFunction(function.Name);

            Dictionary<string, List<string>> organizedBlocks = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> organizedConditionals = OrganizeConditionals(function);
            Dictionary<string, List<string>> organizedLoops = OrganizeLoops(function);

            List<string> orderedLabels = new List<string>();

            organizedConditionals.ToList().ForEach(x => organizedBlocks.Add(x.Key, x.Value));
            organizedLoops.ToList().ForEach(x => organizedBlocks.Add(x.Key, x.Value));

            asmFunction.PopulateBlocks(organizedConditionals.Keys.ToList());
            asmFunction.PopulateBlocks(organizedLoops.Keys.ToList());

            int conditionalsHit = 0;
            int loopsHit = 0;

            foreach (string line in function.Code)
            {
                List<string> linePieces = line.ExplodeAndClean(' ');

                //find where this code goes
                string labelBlock = organizedBlocks.FindBlockForCode(line);

                //if code isnt in a conditional block, then just put it in the main function code
                if (labelBlock == null)
                {
                    labelBlock = function.Name;
                }

                bool foundKeyword = Keyword.IsMatch(linePieces[0], out string keyword);

                //generate code for function call
                if (line.IsFunctionCall() != AssignmentTypes.None && !foundKeyword)
                {
                    List<string> nativeASMCode = GenerateNativeCall(function, line);
                    asmFunction.LabelBlocks[labelBlock].AddRange(nativeASMCode);
                }
                //do conditional code generation
                else if (keyword == "if")
                {
                    Conditional thisConditional = function.Conditionals[conditionalsHit];
                    string label;
                    if (thisConditional.Parent != null)
                    {
                        label = $"{function.Name}_nested_{thisConditional.Parent.Index}_if_end_{thisConditional.Index}";
                    }
                    else
                    {
                        label = $"{function.Name}_if_end_{thisConditional.Index}";
                    }

                    if (thisConditional.Logic.FirstCondition == null)
                    {
                        throw new Exception("Expected first conditional logic, got null");
                    }

                    asmFunction.LabelBlocks[labelBlock].Add(thisConditional.ParseConditionalLogic(thisConditional.Logic.FirstCondition));

                    //if it's null, then the user is doing something like if (!someVar) or if (someVar)
                    if (thisConditional.Logic.SecondCondition == null)
                    {
                        if (thisConditional.Logic.FirstCondition.GetDataType() != "bool")
                        {
                            throw new Exception("Trying to use short-hand conditional logic on non-bool value");
                        }
                        if (thisConditional.Logic.LogicType == ConditionalLogicTypes.NotEqual)
                        {
                            asmFunction.LabelBlocks[labelBlock].Add(Bitwise.Generate(BitwiseType.Not));
                        }
                        else if (thisConditional.Logic.LogicType != ConditionalLogicTypes.Equal)
                        {
                            throw new Exception("Trying to use short-hand conditional logic on invalid logic type");
                        }
                        asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.False, label));
                    }
                    else
                    {
                        asmFunction.LabelBlocks[labelBlock].Add(thisConditional.ParseConditionalLogic(thisConditional.Logic.SecondCondition));
                        switch (thisConditional.Logic.LogicType)
                        {
                            //each one will equate to the inverse of it so it jumps properly
                            case ConditionalLogicTypes.Equal:
                                asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.NotEqual, label));
                                break;
                            case ConditionalLogicTypes.NotEqual:
                                asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.Equal, label));
                                break;
                            case ConditionalLogicTypes.LessThan:
                                asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.GreaterThan, label));
                                break;
                            case ConditionalLogicTypes.LessThanEqual:
                                asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.GreaterThanEqual, label));
                                break;
                            case ConditionalLogicTypes.GreaterThan:
                                asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.LessThan, label));
                                break;
                            case ConditionalLogicTypes.GreaterThanEqual:
                                asmFunction.LabelBlocks[labelBlock].Add(Jump.Generate(JumpType.LessThanEqual, label));
                                break;
                        }
                    }
                    conditionalsHit++;
                }
                else if (keyword == "for" || keyword == "while")
                {
                    ControlLoop thisLoop = function.Loops[loopsHit];
                    string label = $"{function.Name}_for_loop_{thisLoop.Index}";

                    //setup the loop logic
                    switch (thisLoop.Type)
                    {
                        case ControlLoopTypes.For:
                            ForLogic logic = (ForLogic)thisLoop.Logic;
                            //will clean this up later
                            asmFunction.LabelBlocks[labelBlock].Add(Push.Generate(logic.InitialIteratorValue.ToString()));
                            Variable localVar = function.LocalVariables.GetLocalVariable(logic.IteratorVariable);
                            asmFunction.LabelBlocks[labelBlock].Add(FrameVar.Set(localVar));
                            break;
                    }
                    asmFunction.LabelBlocks[labelBlock].Add($":{label}");
                    loopsHit++;
                }
                else if (line.IsAssignment() != AssignmentTypes.None)
                {
                    List<string> instructions = GenerateAssignmentInstructions(function, line);
                    asmFunction.LabelBlocks[labelBlock].AddRange(instructions);
                }
            }

            //remove the main function label (we already output it at the top)
            KeyValuePair<string, List<string>> mainBlock = asmFunction.LabelBlocks.Where(a => a.Key == function.Name).First();
            asmFunction.LabelBlocks.Remove(function.Name);

            //add the main code to the assembly
            asmCode.AddRange(mainBlock.Value);

            //order the blocks based on their index
            var orderedBlocks = asmFunction.LabelBlocks.OrderBlocks(function);

            foreach (KeyValuePair<string, List<string>> blocks in orderedBlocks)
            {
                if (blocks.Key != function.Name)
                {
                    asmCode.Add("");
                    asmCode.Add($":{blocks.Key}");
                }

                foreach (string line in blocks.Value)
                {
                    asmCode.Add(line);
                }
            }
            asmCode.Add($"Return 0 0");
            return asmCode;
        }

        //creates a CallNative instruction
        public static List<string> GenerateNativeCall(Function function, string code)
        {
            code = code.Trim();
            List<string> nativeCall = new List<string>();
            NativeCall call = new NativeCall();
            bool hasReturnValue = false;
            FunctionCall callInfo = code.GetFunctionCallInfo();

            if (callInfo.Arguments.Count > 0)
            {
                foreach (Argument arg in callInfo.Arguments)
                {
                    Variable localVar = function.LocalVariables.Where(s => s.Value == arg.Value).FirstOrDefault();
                    nativeCall.Add(GeneratePushInstruction(function, arg));
                }
            }
            if (!Native.IsFunctionANative(callInfo.FunctionName))
            {
                nativeCall.Add($"Call @{callInfo.FunctionName}");
            }
            else
            {
                nativeCall.Add($"CallNative \"{callInfo.FunctionName}\" {callInfo.Arguments.Count} {Convert.ToInt32(hasReturnValue)}");
            }
            if (callInfo.HasReturnValue)
            {
                Variable returnVar = function.LocalVariables.Where(a => a.Value == callInfo.ReturnVariableName).FirstOrDefault();
                if (returnVar == null)
                {
                    throw new Exception("Return variable not found");
                }
                nativeCall.Add($"setF1 {returnVar.FrameId}");
            }
            return nativeCall;
        }

        public static string GeneratePushInstruction(Function function, Argument arg)
        {
            string code = Push.Generate(arg.Value, arg.ValueType);
            if (code != null)
            {
                return code;
            }
            Variable localVar = function.LocalVariables.Where(s => s.Value == arg.Value).FirstOrDefault();
            if (localVar == null)
            {
                throw new Exception("Variable used for push instruction not found");
            }
            return FrameVar.Get(localVar);
        }

        public static List<string> GenerateAssignmentInstructions(Function function, string line)
        {
            List<string> final = new List<string>();

            Assignment assignment = line.GetAssignmentInfo();

            Variable variable = function.LocalVariables.GetLocalVariable(assignment.AssignedVariable);

            final.Add(Push.Generate(assignment.AssignedValue, assignment.AssignedValueType));
            final.Add(FrameVar.Set(variable));

            return final;
        }

        public static Dictionary<string, List<string>> OrganizeConditionals(Function function)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            foreach (Conditional conditional in function.Conditionals)
            {
                string conditionalEndLabel;
                if (conditional.Parent != null)
                {
                    conditionalEndLabel = $"{function.Name}_nested_{conditional.Parent.Index}_if_end_{conditional.Index}";
                }
                else
                {
                    conditionalEndLabel = $"{function.Name}_if_end_{conditional.Index}";
                }
                List<string> logicCode = function.Code.GetRange(conditional.CodeStartLine, ((int)conditional.CodeEndLine - conditional.CodeStartLine) + 1);

                //see if this conditional is a nested one
                if (conditional.Parent == null)
                {
                    if (function.Conditionals.AreThereAnyParentConditionalsAfterThisParent(conditional))
                    {
                        Conditional nextParent = function.Conditionals.GetNextParentConditional(conditional);
                        int index = (int)conditional.CodeEndLine + 1;
                        int count = ((int)nextParent.CodeEndLine + 1) - ((int)conditional.CodeEndLine + 1);
                        result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
                    }
                    else
                    {
                        int index = (int)conditional.CodeEndLine + 1;
                        int count = (function.Code.Count) - ((int)conditional.CodeEndLine + 1);
                        result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
                    }
                }
                else
                {
                    Conditional nextConditional = function.Conditionals.GetNextNonParentConditional(conditional);
                    if (function.Conditionals.DoesConditionalHaveChildren(conditional))
                    {
                        nextConditional = function.Conditionals.GetNextConditionalWithSameParent(conditional);
                        if (nextConditional == null)
                        {
                            nextConditional = conditional.Parent;
                            int index = (int)conditional.CodeEndLine + 1;
                            int count = ((int)nextConditional.CodeEndLine + 1) - ((int)conditional.CodeEndLine + 1);
                            result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
                        }
                        else
                        {
                            int index = (int)conditional.CodeEndLine + 1;
                            int count = ((int)nextConditional.CodeEndLine + 1) - ((int)conditional.CodeEndLine + 1);
                            result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
                        }

                    }
                    else if (nextConditional == null || nextConditional.Parent != conditional.Parent)
                    {
                        int index = (int)conditional.CodeEndLine + 1;
                        int count = ((int)conditional.Parent.CodeEndLine + 1) - ((int)conditional.CodeEndLine + 1);
                        result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
                    }
                    //might need to enable this in the future
                    //else if (nextConditional.Parent == conditional.Parent)
                    //{
                    //    int index = (int)conditional.CodeEndLine + 1;
                    //    int count = ((int)nextConditional.CodeEndLine + 1) - ((int)conditional.CodeEndLine + 1);
                    //    result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
                    //}
                }

            }
            return result;
        }

        public static Dictionary<string, List<string>> OrganizeLoops(Function function)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            foreach (ControlLoop loop in function.Loops)
            {
                string label = $"{ function.Name }_for_loop_{ loop.Index}";
                List<string> loopCode = function.Code.GetRange(loop.CodeStartLine, ((int)loop.CodeEndLine - loop.CodeStartLine) + 1);
                result.Add(label, loopCode);
            }

            return result;

        }

        //public Dictionary<string, List<string>> OrganizeLoops(Function function)
        //{
        //    Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

        //    foreach (ControlLoop loop in function.Loops)
        //    {
        //        string loopLabel;
        //        if (loop.Parent != null)
        //        {
        //            loopLabel = $"{function.Name}_nested_{loop.Parent.Index}_for_loop_{loop.Index}";
        //        }
        //        else
        //        {
        //            loopLabel = $"{function.Name}_for_loop_{loop.Index}";
        //        }
        //        List<string> logicCode = function.Code.GetRange(loop.CodeStartLine, ((int)loop.CodeEndLine - loop.CodeStartLine) + 1);

        //        if (loop.Parent == null)
        //        {
        //            if (function.Loops.AreThereAnyParentLoopsAfterThisParent(loop))
        //            {
        //                ControlLoop nextParent = function.Loops.GetNextParentLoop(loop);
        //                int index = (int)loop.CodeEndLine + 1;
        //                int count = ((int)nextParent.CodeEndLine + 1) - ((int)loop.CodeEndLine + 1);
        //                //result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
        //            }
        //            else
        //            {
        //                int index = (int)loop.CodeEndLine + 1;
        //                int count = (function.Code.Count) - ((int)loop.CodeEndLine + 1);
        //                //result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
        //            }
        //        }
        //        else
        //        {
        //            ControlLoop nextLoop = function.Conditionals.GetNextNonParentConditional(conditional);
        //            if (function.Conditionals.DoesConditionalHaveChildren(conditional))
        //            {
        //                nextLoop = function.Conditionals.GetNextConditionalWithSameParent(conditional);
        //                if (nextLoop == null)
        //                {
        //                    nextLoop = loop.Parent;
        //                    int index = (int)loop.CodeEndLine + 1;
        //                    int count = ((int)nextLoop.CodeEndLine + 1) - ((int)loop.CodeEndLine + 1);
        //                    //result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
        //                }
        //                else
        //                {
        //                    int index = (int)loop.CodeEndLine + 1;
        //                    int count = ((int)nextLoop.CodeEndLine + 1) - ((int)loop.CodeEndLine + 1);
        //                   // result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
        //                }

        //            }
        //            else if (nextLoop == null || nextLoop.Parent != loop.Parent)
        //            {
        //                int index = (int)loop.CodeEndLine + 1;
        //                int count = ((int)loop.Parent.CodeEndLine + 1) - ((int)loop.CodeEndLine + 1);
        //                //result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
        //            }
        //            //might need to enable this in the future
        //            //else if (nextConditional.Parent == conditional.Parent)
        //            //{
        //            //    int index = (int)conditional.CodeEndLine + 1;
        //            //    int count = ((int)nextConditional.CodeEndLine + 1) - ((int)conditional.CodeEndLine + 1);
        //            //    result.Add(conditionalEndLabel, function.Code.GetRange(index, count));
        //            //}
        //        }
        //    }
        //}
    }
}
