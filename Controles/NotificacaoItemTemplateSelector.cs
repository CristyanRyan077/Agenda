using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AgendaNovo.Controles
{
    public class NotificacaoItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? NotificacaoTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Notificacao)
                return NotificacaoTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
