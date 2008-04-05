using System;
using System.Text;
//+
public partial class _Default : System.Web.UI.Page
{
    //- #OnInit- //
    protected override void OnInit(EventArgs e)
    {
        Load += new EventHandler(Page_Load);
        //+
        base.OnInit(e);
    }

    //- #Page_Load- //
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/png";
    }
}