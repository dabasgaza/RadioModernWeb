// ============================================================
// FluentValidationActionFilter — التحقق من الصحة
// ============================================================
// المسؤولية: تعريف التحقق من الصحة.
// ============================================================
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Radio.Web.Filters
{
    /// <summary>
    /// FluentValidationActionFilter: صنف FluentValidationActionFilter.
    /// <summary>
    /// صنف التحقق من الصحة.
    /// </summary>
    /// <summary>
    /// صنف التحقق من الصحة.
    /// </summary>
    /// <summary>
    /// صنف التحقق من الصحة.
    /// </summary>
    /// <summary>
    /// صنف التحقق من الصحة.
    /// </summary>
    /// <summary>
    /// صنف التحقق من الصحة.
    /// </summary>
    /// <summary>
    /// صنف التحقق من الصحة.
    /// </summary>
    /// </summary>
    public class FluentValidationActionFilter : IAsyncActionFilter
    {
        /// <summary>
        /// معالجة Radio.Web.
        /// <summary>
        /// عند Action Execution Async.
        /// </summary>
        /// <summary>
        /// عند Action Execution Async.
        /// </summary>
        /// <summary>
        /// عند Action Execution Async.
        /// </summary>
        /// <summary>
        /// عند Action Execution Async.
        /// </summary>
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var serviceProvider = context.HttpContext.RequestServices;

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var argumentType = argument.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

                if (serviceProvider.GetService(validatorType) is IValidator validator)
                {
                    var contextType = typeof(ValidationContext<>).MakeGenericType(argumentType);
                    if (Activator.CreateInstance(contextType, argument) is IValidationContext validationContext)
                    {
                        var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                        if (!validationResult.IsValid)
                        {
                            foreach (var error in validationResult.Errors)
                            {
                                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                            }
                        }
                    }
                }
            }

            await next();
        }
    }
}
