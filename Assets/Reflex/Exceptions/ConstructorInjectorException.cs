using System;
using System.Linq;
using Reflex.Extensions;

namespace Reflex.Exceptions
{
    internal sealed class ConstructorInjectorException : Exception
    {
        public ConstructorInjectorException(Type type, Exception exception, Type[] constructorParameters) : base(BuildMessage(type, exception, constructorParameters), exception)
        {
        }

        // Crash reporters only ever see the top-level Message of an uncaught exception, so the real cause has to
        // be inside this string. Under IL2CPP the activator constructs by reflection, so a throwing constructor
        // arrives here as a TargetInvocationException ("Exception has been thrown by the target of an
        // invocation.") and reporting its Message says nothing about what actually failed. Report the innermost
        // exception; the full chain is still carried as InnerException.
        private static string BuildMessage(Type type, Exception exception, Type[] constructorParameters)
        {
            var constructorSignature = $"{type.Name} ({string.Join(", ", constructorParameters.Select(t => t.Name))})";
            var cause = RootCauseOf(exception);
            return $"{cause.GetType().Name}: {cause.Message} occurred while instantiating object type '{type.GetFullName()}' using constructor {constructorSignature}";
        }

        private static Exception RootCauseOf(Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception;
        }
    }
}