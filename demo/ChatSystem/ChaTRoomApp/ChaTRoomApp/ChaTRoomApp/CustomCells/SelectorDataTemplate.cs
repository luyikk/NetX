using ChaTRoomApp.Models;
using Xamarin.Forms;

namespace ChaTRoomApp
{
    public class SelectorDataTemplate : DataTemplateSelector
    {
        private readonly DataTemplate textInDataTemplate;
        private readonly DataTemplate textOutDataTemplate;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as MessageTable;
            if (messageVm == null)
                return null;
            return messageVm.IsRight ? this.textInDataTemplate : this.textOutDataTemplate;
        }


        public SelectorDataTemplate()
        {
            this.textInDataTemplate = new DataTemplate(typeof(TextInViewCell));
            this.textOutDataTemplate = new DataTemplate(typeof(TextOutViewCell));
        }

    }
}
