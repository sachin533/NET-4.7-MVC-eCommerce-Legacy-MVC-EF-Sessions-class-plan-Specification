using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Common
{
    /// <summary>
    /// Lightweight operation result used by services for non-viewmodel outcomes.
    /// </summary>
    public class ServiceResult
    {
        public ServiceResult()
        {
            Errors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool Success { get; set; }
        public string Message { get; set; }
        public IDictionary<string, string> Errors { get; set; }

        public static ServiceResult Ok(string message = null)
        {
            return new ServiceResult { Success = true, Message = message };
        }

        public static ServiceResult Fail(string message, string key = null)
        {
            var result = new ServiceResult { Success = false, Message = message };
            if (!string.IsNullOrEmpty(key))
            {
                result.Errors[key] = message;
            }
            return result;
        }
    }
}
