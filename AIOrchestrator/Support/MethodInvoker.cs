#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator.Support;

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class MethodInvoker
{
    public static object Execute<T>(string instructionJson, T targetInstance) =>
        Execute(Deserialize(instructionJson), targetInstance);

    public static object Execute<T>(FunctionCall instruction, T targetInstance)
    {
        var method =
            typeof(T).GetMethod(
                instruction.Function,
                BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
            )
            ?? throw new MissingMethodException(
                $"Method {instruction.Function} not found in {typeof(T).Name}."
            );

        var parameters = ConvertParametersForMethod(instruction.Parameters, method);

        return method.Invoke(targetInstance, parameters)!;
    }

    public static FunctionCall Deserialize(string jsonInstruction) =>
        JsonSerializer.Deserialize<FunctionCall>(jsonInstruction)!;

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

    public class FunctionCall
    {
        [JsonPropertyOrder(1)]
        public required string Function { get; set; }

        [JsonPropertyOrder(2)]
        public object[] Parameters { get; set; } = [];
    }

    public class FunctionResponse : FunctionCall
    {
        [JsonPropertyOrder(3)]
        public required string Response { get; set; } = string.Empty;
    }
}
