using System.Windows.Controls;

namespace Tennisi.Xunit.UIParallelTestFramework.Tests.Forms;

public partial class TestWindow
{
    public TestWindow()
    {
        InitializeComponent();
        SampleButton.Click += (_, _) => { };
    }
    
    public TextBox PublicTextBox => FindName("SampleTextBox") as TextBox ?? throw new InvalidOperationException();
    public Button PublicButton => FindName("SampleButton") as Button ?? throw new InvalidOperationException();
}