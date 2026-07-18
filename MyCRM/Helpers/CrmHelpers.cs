using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyCRM.Data.Models;

namespace MyCRM.Helpers;

public static partial class CrmHelpers
{
    public static void StampCreate(BaseEntity entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = null;
    }

    public static void StampUpdate(BaseEntity entity)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static string DisplayName(Enum value) =>
        SplitPascal().Replace(value.ToString(), " $1");

    public static SelectList EnumSelectList<TEnum>(TEnum? selected = null)
        where TEnum : struct, Enum
    {
        var items = Enum.GetValues<TEnum>()
            .Select(v => new SelectListItem(DisplayName(v), Convert.ToInt32(v).ToString()));

        return new SelectList(items, "Value", "Text",
            selected.HasValue ? Convert.ToInt32(selected.Value).ToString() : null);
    }

    public static SelectList NullableEnumSelectList<TEnum>(TEnum? selected = null, string emptyText = "(None)")
        where TEnum : struct, Enum
    {
        var items = new List<SelectListItem> { new(emptyText, "") };
        items.AddRange(Enum.GetValues<TEnum>()
            .Select(v => new SelectListItem(DisplayName(v), Convert.ToInt32(v).ToString())));

        return new SelectList(items, "Value", "Text",
            selected.HasValue ? Convert.ToInt32(selected.Value).ToString() : "");
    }

    [GeneratedRegex(@"(\B[A-Z])")]
    private static partial Regex SplitPascal();
}
