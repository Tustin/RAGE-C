using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

//Tustin
namespace RAGE
{
    class Parser
    {
        private readonly StreamReader sourceFile = null;

        private List<string> dataTypes = new List<string>() { "void", "int", "bool", "string" };

        public Parser(StreamReader fileStream)
        {
            this.sourceFile = fileStream;
        }
        private void ResetStream()
        {
            sourceFile.DiscardBufferedData();
            sourceFile.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        public List<string> GenerateEntryPoint(bool networkScript = false)
        {
            List<string> code = new List<string>();
            code.Add(":__script__entry");
            code.Add("Function 0 2 0");
            if (networkScript)
            {
                code.Add("CallNative \"NETWORK_SET_SCRIPT_IS_SAFE_FOR_NETWORK_GAME\" 0 0");
            }
            code.Add("Call @main");
            code.Add("Return 0 0");
            code.Add("");
            return code;
        }

        public Function GetFunctionContents(string functionName)
        {
            Function f = new Function();
            f.Code = new List<string>();
            string line;
            bool foundFunction = false;
            bool inFunctionBlock = false;
            bool inLogicBlock = false;
            int conditionalCount = 0;
            ResetStream();
            while ((line = sourceFile.ReadLine()) != null)
            {
                line = line.Trim();
                List<string> pieces = line.ExplodeAndClean(' ');
                if (!line.Contains(functionName) && !foundFunction)
                {
                    continue;
                }
                else if (!foundFunction)
                {
                    //found it, make sure it's a function
                    if (line.IsFunction())
                    {
                        var matches = line.GetFunctionInfo();
                        f.ReturnType = matches[0];
                        f.Name = matches[1];
                        foundFunction = true;
                    }
                }

                //we found the function and this list contains an open curly bracket and we currently aren't in a block
                //we can assume this is the start
                if (foundFunction && line.Contains('{') && !inFunctionBlock)
                {
                    inFunctionBlock = true;
                    continue;
                }

                string keyword = null;
                bool foundKeyword = Keyword.IsMatch(pieces[0], out keyword);
                //we found the function, are currently in it but we found an opening bracket
                //we can assume this is an if logic block
                if (foundFunction && foundKeyword && keyword == "if" && inFunctionBlock)
                {
                    List<string> elements = line.ExplodeAndClean(' ');
                    //makes it so you can either put the { on the same line as the if or on the line after
                    //only assumes that the { actually exists so if it doesn't, the code will be fucked up
                    int startPosition = f.Code.Count + 1;
                    if (!line.Contains("{"))
                    {
                        startPosition = f.Code.Count + 2;
                    }
                    Conditional conditional = new Conditional(ConditionalTypes.JustIf, startPosition, null);
                    conditional.Index = conditionalCount++;
                    conditional.Function = f;
                    //lets see if this conditional is nested
                    if (f.Conditionals.AreThereAnyParentConditionals(conditional) && f.AreThereAnyUnclosedLogicBlocks())
                    {
                        Conditional lastConditional = f.Conditionals.GetLastConditional(conditional);
                        //since the last one isnt closed, we can assume its the parent of this one
                        if (lastConditional.CodeEndLine == null)
                        {
                            conditional.Parent = lastConditional;
                        }
                        else
                        {
                            Conditional lastParent = f.Conditionals.GetLastParentConditional();
                            conditional.Parent = lastParent;
                        }
                    }

                    conditional.Logic = ConditionalLogic.Parse(line);

                    f.Conditionals.Add(conditional);
                    inLogicBlock = true;
                    f.Code.Add(line);
                    continue;
                }

                //we found the function, are currently in it AND we're also in a logic block - we can assume this is an ending to a logic block        
                if (foundFunction && line.Contains('}') && inFunctionBlock && f.AreThereAnyUnclosedLogicBlocks())
                {
                    if (f.Conditionals.Count == 0)
                    {
                        throw new Exception("No conditionals have been found prior, how did you get this?");
                    }
                    int lastUnclosedBlock = f.GetIndexOfLastUnclosedLogicBlock();
                    f.Conditionals[lastUnclosedBlock].CodeEndLine = f.Code.Count - 1;
                    inLogicBlock = false;
                    f.Code.Add(line);
                    continue;
                }

                //we found the function, we're in the function code block and it contains an assignment
                if (foundFunction && inFunctionBlock && line.Contains("=") && !line.Contains("=="))
                {
                    List<string> elements = line.Split('=').ToList();
                    List<string> assignmentPieces = elements[0].ExplodeAndClean(' ');
                    assignmentPieces[0].ToLower();
                    string type = assignmentPieces.Intersect(dataTypes).FirstOrDefault();
                    if (type == null)
                    {
                        throw new Exception("Type hasnt been defined yet");
                    }
                    f.LocalVariables.Add(new Variable()
                    {
                        Value = assignmentPieces[1],
                        ValueType = type,
                        FrameId = f.frameCount++
                    });
                }

                //we found the function, we're in the function code block and its not the end of the function
                if (foundFunction && inFunctionBlock && !line.Contains('}'))
                {
                    f.Code.Add(line);
                    continue;
                }

                //we found the function, we're in the function code block and we're not in a logic block but we found a closing curling bracket - we can assume its the end of the function
                if (foundFunction && inFunctionBlock && line.Contains('}') && !inLogicBlock)
                {
                    inFunctionBlock = false;
                    return f;
                }
            }
            return null;
        }

        public List<Function> GetAllFunctions()
        {
            List<Function> scriptFunctions = new List<Function>();
            string line;
            ResetStream();
            while ((line = sourceFile.ReadLine()) != null)
            {
                if (line.IsFunction())
                {
                    List<string> functionInfo = line.GetFunctionInfo();
                    if (!dataTypes.Any(a => a == functionInfo[0]))
                    {
                        throw new Exception($"Return type for function '{functionInfo[1]}' not supported");
                    }
                    scriptFunctions.Add(GetFunctionContents(functionInfo[1]));
                }
            }
            return scriptFunctions;
        }

        public List<string> GenerateASMFunction(Function function)
        {
            List<string> asmCode = new List<string>();
            asmCode.Add(":" + function.Name);
            asmCode.Add($"Function 0 {function.frameCount} 0");
            AssemblyFunction asmFunction = new AssemblyFunction(function.Name);
            //Dictionary<string, List<string>> conditionalBlocks = new Dictionary<string, List<string>>(function.Conditionals.Count);
            Dictionary<string, List<string>> organizedConditionals = OrganizeConditionals(function);
            Dictionary<string, List<string>> asmConditionals = organizedConditionals.Where(a => a.Key != null).ToDictionary(a => a.Key, a => new List<string>());
            asmFunction.PopulateBlocks(organizedConditionals.Keys.ToList());
            int conditionalsHit = 0;
            foreach (string line in function.Code)
            {
                List<string> linePieces = line.ExplodeAndClean(' ');
                //find where this code goes
                string labelBlock = organizedConditionals.FindConditionalBlockForCode(line);
                //if code isnt in a conditional block, then just put it in the main function code
                if (labelBlock == null)
                {
                    labelBlock = function.Name;
                }
                string keyword = null;
                bool foundKeyword = Keyword.IsMatch(linePieces[0], out keyword);
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
                        if (!thisConditional.Logic.LogicType)
                        {
                            asmFunction.LabelBlocks[labelBlock].Add("Not");
                            asmFunction.LabelBlocks[labelBlock].Add($"JumpFalse @{label}");
                        }
                    }
                    else
                    {
                        asmFunction.LabelBlocks[labelBlock].Add(thisConditional.ParseConditionalLogic(thisConditional.Logic.SecondCondition));
                    }

                    asmFunction.LabelBlocks[labelBlock].Add($"JumpNE @{label}");
                    conditionalsHit++;

                    //string firstCondition = thisConditional.Logic.FirstCondition;
                    //if (function.LocalVariables.IsLocalVariable(firstCondition))
                    //{
                    //    Variable localVar = function.LocalVariables.GetLocalVariable(firstCondition);
                    //    asmFunction.LabelBlocks[labelBlock].Add($"getF1 {localVar.FrameId}");
                    //}
                    //else
                    //{
                    //    //turn any hashes into int format
                    //    if (firstCondition.Contains("0x"))
                    //    {
                    //        firstCondition = firstCondition.Replace("0x", "");
                    //        firstCondition = int.Parse(firstCondition, System.Globalization.NumberStyles.HexNumber).ToString();
                    //    }
                    //    asmFunction.LabelBlocks[labelBlock].Add(GeneratePushInstruction(firstCondition));

                    //}
                    //string secondCondition = thisConditional.Logic.SecondCondition;
                    ////is this a local var? if so, pull it and get the frame var id
                    //if (function.LocalVariables.IsLocalVariable(secondCondition))
                    //{
                    //    Variable localVar = function.LocalVariables.GetLocalVariable(secondCondition);
                    //    asmFunction.LabelBlocks[labelBlock].Add($"getF1 {localVar.FrameId}");
                    //}
                    //else
                    //{
                    //    //if its a hex value, convert it to an int
                    //    if (secondCondition.Contains("0x"))
                    //    {
                    //        secondCondition = secondCondition.Replace("0x", "");
                    //        secondCondition = int.Parse(secondCondition, System.Globalization.NumberStyles.HexNumber).ToString();
                    //    }
                    //    asmFunction.LabelBlocks[labelBlock].Add(GeneratePushInstruction(secondCondition));
                    //}
                }
                else if (line.IsAssignment() != AssignmentTypes.None)
                {
                    List<string> instructions = GenerateAssignmentInstructions(function, line);
                    asmFunction.LabelBlocks[labelBlock].AddRange(instructions);
                }
            }

            KeyValuePair<string, List<string>> mainBlock = asmFunction.LabelBlocks.Where(a => a.Key == function.Name).First();
            asmFunction.LabelBlocks.Remove(function.Name);
            asmCode.AddRange(mainBlock.Value);
            var orderedBlocks = asmFunction.LabelBlocks.OrderBlocks(function);
            foreach (KeyValuePair<string, List<string>> blocks in orderedBlocks)
            {
                if (blocks.Key != function.Name)
                {
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
        public List<string> GenerateNativeCall(Function function, string code)
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
            string code = GeneratePushInstruction(arg.Value, arg.ValueType);
            if (code != null)
            {
                return code;
            }
            Variable localVar = function.LocalVariables.Where(s => s.Value == arg.Value).FirstOrDefault();
            if (localVar == null)
            {
                throw new Exception("Variable used for push instruction not found");
            }
            return $"getF1 {localVar.FrameId}";
        }

        public static string GeneratePushInstruction(string arg)
        {
            return GeneratePushInstruction(arg, arg.GetDataType());
        }

        public static string GeneratePushInstruction(string value, string valueType)
        {
            switch (valueType)
            {
                case "bool":
                return PushInstruction.Bool(value);
                case "float":
                return PushInstruction.Float(value);
                case "string":
                return PushInstruction.String(value);
                case "int":
                return PushInstruction.Int(value);
                default:
                return null;
            }
        }

        public List<string> GenerateAssignmentInstructions(Function function, string line)
        {
            List<string> final = new List<string>();

            Assignment assignment = line.GetAssignmentInfo();

            Variable variable = function.LocalVariables.GetLocalVariable(assignment.AssignedVariable);

            string code = GeneratePushInstruction(assignment.AssignedValue, assignment.AssignedValueType);

            final.Add(code);
            final.Add($"setF1 {variable.FrameId}");

            return final;
        }

        public Dictionary<string, List<string>> OrganizeConditionals(Function function)
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
                    if (function.Conditionals.AreThereAnyParentsAfterThisParent(conditional))
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
    }
}
