using System;
//+
public partial class _Default : System.Web.UI.Page 
{
    //- @Test- //
    public String Test
    {
        get
        {
            return (String)ViewState["Test"];
        }
        set
        {
            ViewState["Test"] = value;
        }
    }

    //- #OnInit- //
    protected override void OnInit(EventArgs e)
    {
        btnCopy.Click += delegate
        {
            Test = "Stuff";
            txtOutput.Text = txtInput.Text;
        };
        base.OnInit(e);
    }
}
