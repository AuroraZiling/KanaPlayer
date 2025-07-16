using System;
using Avalonia.Markup.Xaml;

namespace KanaPlayer.MarkupExtensions;

public class RootObjectProviderExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootObjectProvider)
        {
            return rootObjectProvider.RootObject;
        }
        throw new Exception("Root object provider not found");
    }
}