using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HealthPlus.Conventions;

public class KebabCaseControllerConvention : IControllerModelConvention
{
    private static readonly Regex _pattern = new("(?<=[a-z0-9])([A-Z])", RegexOptions.Compiled);

    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            var template = selector.AttributeRouteModel?.Template;
            if (template is not null && template.Contains("[controller]"))
            {
                var kebab = _pattern.Replace(controller.ControllerName, "-$1").ToLowerInvariant();
                selector.AttributeRouteModel!.Template = template.Replace("[controller]", kebab);
            }
        }
    }
}
