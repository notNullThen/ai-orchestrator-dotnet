namespace AIOrchestrator.Support;

using System;
using System.Reflection;
using System.Text.Json;
using AIOrchestrator.Support.Types;

public static partial class MethodInvoker
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

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
                $"Method {instruction.Function}() not found in {typeof(T).Name} class."
            );

        var parameters = ConvertParametersForMethod(instruction.Parameters, method);

        return method.Invoke(targetInstance, parameters)!;
    }

    public static FunctionCall Deserialize(string jsonInstruction)
    {
        try
        {
            return JsonSerializer.Deserialize<FunctionCall>(
                jsonInstruction,
                _jsonSerializerOptions
            )!;
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Failed to deserialize function call. Response was: {jsonInstruction}",
                exception
            );
        }
    }

    private static object[] ConvertParametersForMethod(object[] rawParameters, MethodInfo method)
    {
        var methodParams = method.GetParameters();
        var convertedParameters = new object[rawParameters.Length];

        for (var i = 0; i < rawParameters.Length; i++)
        {
            var parameterType = methodParams[i].ParameterType;
            convertedParameters[i] = ((JsonElement)rawParameters[i]).Deserialize(
                parameterType,
                _jsonSerializerOptions
            )!;
        }

        return convertedParameters;
    }
}
