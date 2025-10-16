using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AgendaNovo.Helpers
{
    public static class TextHighlight
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TextHighlight),
                new PropertyMetadata(null, OnPropsChanged));

        public static readonly DependencyProperty TermProperty =
            DependencyProperty.RegisterAttached("Term", typeof(string), typeof(TextHighlight),
                new PropertyMetadata(null, OnPropsChanged));

        public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
        public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

        public static string GetTerm(DependencyObject obj) => (string)obj.GetValue(TermProperty);
        public static void SetTerm(DependencyObject obj, string value) => obj.SetValue(TermProperty, value);

        private static void OnPropsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock tb) return;

            var text = GetText(tb) ?? string.Empty;
            var term = GetTerm(tb);
            tb.Inlines.Clear();

            if (string.IsNullOrWhiteSpace(term))
            {
                tb.Inlines.Add(new Run(text));
                return;
            }

            int idx = 0;
            int len = text.Length;
            var comparison = StringComparison.OrdinalIgnoreCase;

            while (idx < len)
            {
                int hit = text.IndexOf(term, idx, comparison);
                if (hit < 0)
                {
                    tb.Inlines.Add(new Run(text.Substring(idx)));
                    break;
                }
                if (hit > idx)
                    tb.Inlines.Add(new Run(text.Substring(idx, hit - idx)));

                var run = new Run(text.Substring(hit, term.Length))
                {
                    Background = new SolidColorBrush(Color.FromArgb(160, 255, 235, 59)), // amarelinho
                    FontWeight = FontWeights.SemiBold
                };
                tb.Inlines.Add(run);
                idx = hit + term.Length;
            }
        }
    }
}
