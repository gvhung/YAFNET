/* Yet Another Forum.NET
 * Copyright (C) 2006-2008 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using YAF.Classes.Utils;
using YAF.Classes.Data;

namespace YAF.Pages.Admin
{
	public partial class bbcode_import : YAF.Classes.Base.AdminPage
	{
		protected void Page_Load( object sender, EventArgs e )
		{
			if ( !IsPostBack )
			{
				PageLinks.AddLink( PageContext.BoardSettings.Name, YAF.Classes.Utils.YafBuildLink.GetLink( YAF.Classes.Utils.ForumPages.forum ) );
				PageLinks.AddLink( "Administration", YAF.Classes.Utils.YafBuildLink.GetLink( YAF.Classes.Utils.ForumPages.admin_admin ) );
				PageLinks.AddLink( "Import Custom BBCode", "" );
			}
		}

		protected void Cancel_OnClick( object sender, System.EventArgs e )
		{
			YafBuildLink.Redirect( YAF.Classes.Utils.ForumPages.admin_bbcode );
		}

		protected void Import_OnClick( object sender, System.EventArgs e )
		{
			// import selected file (if it's the proper format)...
			if ( importFile.PostedFile.ContentType == "text/xml" )
			{
				try
				{
					int importedCount = YAF.Classes.Data.Import.DataImport.BBCodeExtensionImport( PageContext.PageBoardID, importFile.PostedFile.InputStream );

					if ( importedCount > 0 )
					{
						PageContext.AddLoadMessageSession( String.Format( "{0} new custom bbcode(s) imported successfully.", importedCount ) );
					}
					else
					{
						PageContext.AddLoadMessageSession( String.Format( "Nothing imported: no new custom bbcode was found in the upload.", importedCount ) );
					}

					YafBuildLink.Redirect( ForumPages.admin_bbcode );
				}
				catch ( Exception x )
				{
					PageContext.AddLoadMessage( "Failed to import: " + x.Message );
				}
			}
		}
	}
}
