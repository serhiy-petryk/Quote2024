using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Data.Helpers
{
    public static class Logger
    {
        public static Action<string> DefaultShowStatus = null;
        private static readonly ConcurrentBag<MessageAddedEventArgs> Messages = new ConcurrentBag<MessageAddedEventArgs>();

        public static void AddMessage(string message, Action<string> fnShowStatus = null)
        {
            var sf = new StackFrame(1);
            var method = sf.GetMethod();
            var callMethodName = method.DeclaringType?.Name + "." + method.Name;

            var oMessage = new MessageAddedEventArgs(message, callMethodName);
            Messages.Add(oMessage);
            (fnShowStatus ?? DefaultShowStatus)?.Invoke(oMessage.FullMessage);
        }

        public class MessageAddedEventArgs : EventArgs
        {
            public readonly DateTime Date = DateTime.Now;
            public readonly string MethodName;
            public readonly string Message;
            public string FullMessage => $"{MethodName}. {Message}";
            public MessageAddedEventArgs(string msg, string methodName)
            {
                Message = msg;
                MethodName = methodName;
                if (Message.StartsWith("!"))
                    Debug.Print($"LOGGER: {FullMessage}");
            }
        }
    }
}
