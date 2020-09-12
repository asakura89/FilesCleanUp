using System;
using System.Linq;
using ConsoleApp.Validator;

namespace FilesCleanUp.Validator {
    class ModeValidator : IInputValidator {
        public ModeValidator(String invalidMessage) {
            InvalidMessage = invalidMessage;
        }

        public String InvalidMessage { get; }
        public Boolean Validate(String value) =>
            !String.IsNullOrEmpty(value.Trim(' ')) &&
            new[] { "l", "d" }.Contains(value.Trim(' ').ToLowerInvariant());
    }
}
