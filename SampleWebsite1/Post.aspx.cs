using System;
using System.Text;
//+
public partial class _Default : System.Web.UI.Page
{
    //- #OnInit -//
    protected override void OnInit(EventArgs e)
    {
        Load += new EventHandler(Page_Load);
        //+
        base.OnInit(e);
    }

    //- #Page_Load -//
    protected void Page_Load(Object sender, EventArgs e)
    {
        if (Request.InputStream != null && Request.InputStream.Length > 0)
        {
            Byte[] buffer = new Byte[Request.InputStream.Length];
            Request.InputStream.Read(buffer, 0, (Int32)Request.InputStream.Length);
            String data = ASCIIEncoding.UTF8.GetString(buffer);
            litData.Text = String.Format("Your data was: {0}", data);
        }
        else
        {
            litData.Text = String.Format("There was no POST.");
        }
    }
}