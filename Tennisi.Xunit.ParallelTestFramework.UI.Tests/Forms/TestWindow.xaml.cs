using System.Windows;
using System.Windows.Controls;

namespace Tennisi.Xunit.ParallelTestFramework.UI.Tests.Forms;

/// <summary>
/// Interaction logic for TestWindow.xaml
/// </summary>
public partial class TestWindow : Window
{
    public string OutputText { get; private set; }

    public TestWindow(string outputText)
    {
        OutputText = outputText;
        InitializeComponent();
        SampleButton.Click += (sender, args) => OutputText = SampleTextBox.Text;
    }
    
    public TextBox PublicTextBox => FindName("SampleTextBox") as TextBox ?? throw new InvalidOperationException();
    public Button PublicButton => FindName("SampleButton") as Button ?? throw new InvalidOperationException();
}