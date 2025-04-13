#pragma warning disable IDE0210 // Convert to top-level statements
#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator.Support;

using System;
using System.Reflection;
using System.Text.Json;

sealed class MethodInvoker
{
    public static Instruction[] DeserializeArray(string jsonInstruction) => JsonSerializer.Deserialize<Instruction[]>(jsonInstruction)!;

    public static object Execute<T>(Instruction instruction, T targetInstance)
    {
        var method = typeof(T).GetMethod(instruction.Function, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMethodException($"Method {instruction.Function} not found in {typeof(T).Name}.");

        var parameters = ConvertParametersForMethod(instruction.Parameters, method);

        return method.Invoke(targetInstance, parameters)!;
    }


    private static object[] ConvertParametersForMethod(object[] rawParameters, MethodInfo method)
    {
        var methodParams = method.GetParameters();
        var convertedParameters = new object[rawParameters.Length];

        for (var i = 0; i < rawParameters.Length; i++)
        {
            var parameterType = methodParams[i].ParameterType;
            convertedParameters[i] = ((JsonElement)rawParameters[i]).Deserialize(parameterType)!;
        }

        return convertedParameters;
    }

    public sealed class Instruction
    {
        public required string Function { get; set; }
        public required object[] Parameters { get; set; }
    }
}