/* Yet Another Forum.net
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
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Specialized;
using YAF.Classes.Data;

namespace YAF.Classes.Utils
{
	/// <summary>
	/// Summary description for General Utils.
	/// </summary>
	public static class General
	{
		/// <summary>
		/// Converts an array of strings into a ulong representing a 4 byte IP address
		/// </summary>
		/// <param name="ip">string array of numbers</param>
		/// <returns>ulong represending an encoding IP address</returns>
		static public ulong Str2IP( String [] ip )
		{
			if ( ip.Length != 4 )
				throw new Exception( "Invalid ip address." );

			ulong num = 0;
			for ( int i = 0; i < ip.Length; i++ )
			{
				num <<= 8;
				num |= ulong.Parse( ip [i] );
			}
			return num;
		}

		static public ulong IPStrToLong( string ipAddress )
		{
			// not sure why it gives me this for local users on firefox--but it does...
			if ( ipAddress == "::1" ) ipAddress = "127.0.0.1";

			string [] ip = ipAddress.Split( '.' );
			return Str2IP( ip );
		}

		/// <summary>
		/// Verifies that an ip and mask aren't banned
		/// </summary>
		/// <param name="ban">Banned IP</param>
		/// <param name="chk">IP to Check</param>
		/// <returns>true if it's banned</returns>
		static public bool IsBanned( string ban, string chk )
		{
			string bannedIP = ban.Trim();
			if ( chk == "::1" ) chk = "127.0.0.1";

			String [] ipmask = bannedIP.Split( '.' );
			String [] ip = bannedIP.Split( '.' );

			for ( int i = 0; i < ipmask.Length; i++ )
			{
				if ( ipmask [i] == "*" )
				{
					ipmask [i] = "0";
					ip [i] = "0";
				}
				else
					ipmask [i] = "255";
			}

			ulong banmask = Str2IP( ip );
			ulong banchk = Str2IP( ipmask );
			ulong ipchk = Str2IP( chk.Split( '.' ) );

			return ( ipchk & banchk ) == banmask;
		}

		static public string GetSafeRawUrl()
		{
			return GetSafeRawUrl( System.Web.HttpContext.Current.Request.RawUrl );
		}

		/// <summary>
		/// Cleans up a URL so that it doesn't contain any problem characters.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		static public string GetSafeRawUrl( string url )
		{
			string tProcessedRaw = url;
			tProcessedRaw = tProcessedRaw.Replace( "\"", string.Empty );
			tProcessedRaw = tProcessedRaw.Replace( "<", "%3C" );
			tProcessedRaw = tProcessedRaw.Replace( ">", "%3E" );
			tProcessedRaw = tProcessedRaw.Replace( "&", "%26" );
			return tProcessedRaw.Replace( "'", string.Empty );
		}

		/// <summary>
		/// Returns a "random" alpha-numeric string of specified length and characters.
		/// </summary>
		/// <param name="length">the length of the random string</param>
		/// <param name="pickfrom">the string of characters to pick randomly from</param>
		/// <returns></returns>
		public static string GenerateRandomString( int length, string pickfrom )
		{
			Random r = new Random();
			string result = "";
			int picklen = pickfrom.Length - 1;
			int index = 0;
			for ( int i = 0; i < length; i++ )
			{
				index = r.Next( picklen );
				result = result + pickfrom.Substring( index, 1 );
			}
			return result;
		}

		/// <summary>
		/// Gets the CaptchaString using the BoardSettings
		/// </summary>
		/// <returns></returns>
		public static string GetCaptchaString()
		{
			return GenerateRandomString( YafContext.Current.BoardSettings.CaptchaSize, "abcdefghijkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ123456789" );
		}

		/// <summary>
		/// Truncates a string with the specified limits and adds (...) to the end if truncated
		/// </summary>
		/// <param name="input">input string</param>
		/// <param name="limit">max size of string</param>
		/// <returns>truncated string</returns>
		public static string Truncate( string input, int limit )
		{
			string output = input;

			if ( String.IsNullOrEmpty( input ) ) return null;

			// Check if the string is longer than the allowed amount
			// otherwise do nothing
			if ( output.Length > limit && limit > 0 )
			{
				// cut the string down to the maximum number of characters
				output = output.Substring( 0, limit );

				// Check if the space right after the truncate point 
				// was a space. if not, we are in the middle of a word and 
				// need to cut out the rest of it
				if ( input.Substring( output.Length, 1 ) != " " )
				{
					int LastSpace = output.LastIndexOf( " " );

					// if we found a space then, cut back to that space
					if ( LastSpace != -1 )
					{
						output = output.Substring( 0, LastSpace );
					}
				}
				// Finally, add the "..."
				output += "...";
			}
			return output;
		}

		/// <summary>
		/// Truncates a string with the specified limits by adding (...) to the middle
		/// </summary>
		/// <param name="input">input string</param>
		/// <param name="limit">max size of string</param>
		/// <returns>truncated string</returns>
		public static string TruncateMiddle( string input, int limit )
		{
			string output = input;
			const string middle = "...";

			// Check if the string is longer than the allowed amount
			// otherwise do nothing
			if ( output.Length > limit && limit > 0 )
			{
				// figure out how much to make it fit...
				int left = ( limit / 2 ) - ( middle.Length / 2 );
				int right = limit - left - ( middle.Length / 2 );

				if ( ( left + right + middle.Length ) < limit )
				{
					right++;
				}
				else if ( ( left + right + middle.Length ) > limit )
				{
					right--;
				}

				// cut the left side
				output = input.Substring( 0, left );
				// add the middle
				output += middle;
				// add the right side...
				output += input.Substring( input.Length - right, right );
			}
			return output;
		}

		static public bool IsValidEmail( string email )
		{
			return Regex.IsMatch( email, @"^([0-9a-z]+[-._+&])*[0-9a-z]+@([-0-9a-z]+[.])+[a-z]{2,6}$", RegexOptions.IgnoreCase );
		}
		static public bool IsValidURL( string url )
		{
			return Regex.IsMatch( url, @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&%\$#\=~])*[^\.\,\)\(\s]$" );
		}
		static public bool IsValidInt( string val )
		{
			return Regex.IsMatch( val, @"^[1-9]\d*\.?[0]*$" );
		}
		/// <summary>
		/// Searches through SearchText and replaces "bad words" with "good words"
		/// as defined in the database.
		/// </summary>
		/// <param name="searchText">The string to search through.</param>
		static public string BadWordReplace( string searchText )
		{
			string strReturn = searchText;
			RegexOptions options = RegexOptions.IgnoreCase /*| RegexOptions.Singleline | RegexOptions.Multiline*/;

			string cacheKey = YafCache.GetBoardCacheKey( Constants.Cache.ReplaceWords );
			DataTable replaceWordsDT = ( DataTable )YafCache.Current [cacheKey];

			if ( replaceWordsDT == null )
			{
				replaceWordsDT = YAF.Classes.Data.DB.replace_words_list( YafContext.Current.PageBoardID, null );
				YafCache.Current.Add( cacheKey, replaceWordsDT, null, DateTime.Now.AddMinutes( 30 ), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null );
			}
			foreach ( DataRow row in replaceWordsDT.Rows )
			{
				try
				{
					strReturn = Regex.Replace( strReturn, Convert.ToString( row ["badword"] ), Convert.ToString( row ["goodword"] ), options );
				}
#if DEBUG
				catch ( Exception e )
				{
					throw new Exception( "Regular Expression Failed: " + e.Message, e );
				}
#else
				catch (Exception x)
				{
          YAF.Classes.Data.DB.eventlog_create( null, "BadWordReplace", x, YAF.Classes.Data.EventLogTypes.Warning );
				}
#endif
			}

			return strReturn;
		}

		static public string TraceResources()
		{
			System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

			// get a list of resource names from the manifest
			string [] resNames = a.GetManifestResourceNames();

			// populate the textbox with information about our resources
			// also look for images and put them in our arraylist
			string txtInfo = "";

			txtInfo += String.Format( "Found {0} resources\r\n", resNames.Length );
			txtInfo += "----------\r\n";
			foreach ( string s in resNames )
			{
				txtInfo += s + "\r\n";
			}
			txtInfo += "----------\r\n";

			return txtInfo;
		}

		/* Ederon - 7/1/2007 start */

		static public void AddStyleAttributeSize( System.Web.UI.WebControls.WebControl control, string width, string height )
		{
			control.Attributes.Add( "style", String.Format( "width: {0}; height: {1};", width, height ) );
		}

		static public void AddStyleAttributeWidth( System.Web.UI.WebControls.WebControl control, string width )
		{
			control.Attributes.Add( "style", String.Format( "width: {0};", width ) );
		}

		static public void AddStyleAttributeHeight( System.Web.UI.WebControls.WebControl control, string height )
		{
			control.Attributes.Add( "style", String.Format( "height: {0};", height ) );
		}

		/* Ederon - 7/1/2007 end */


		static public void AddOnClickConfirmDialog( object control, string message )
		{
			AddOnClickConfirmDialog( ( System.Web.UI.WebControls.WebControl )control, message );
		}
		static public void AddOnClickConfirmDialog( System.Web.UI.WebControls.WebControl control, string message )
		{
			control.Attributes ["onclick"] = String.Format( "return confirm('{0}');", message );
		}


		/* Ederon - 9/9/2007 */
		static public string ProcessText( string text )
		{
			return ProcessText( text, true );
		}
		static public string ProcessText( string text, bool nullify )
		{
			return ProcessText( text, nullify, true );
		}
		static public string ProcessText( string text, bool nullify, bool trim )
		{
			if ( trim ) text = text.Trim();
			if ( nullify && text.Trim().Length == 0 ) text = null;

			return text;
		}

		/* Ederon : 9/12/2007 */
		static public bool BinaryAnd( object value, object checkAgainst )
		{
			return BinaryAnd( ( int )value, ( int )checkAgainst );
		}
		static public bool BinaryAnd( int value, int checkAgainst )
		{
			return ( value & checkAgainst ) == checkAgainst;
		}


		static public bool CheckPermission( YafContext context, int permission )
		{
			return CheckPermission( context, ( ViewPermissions )permission );
		}
		static public bool CheckPermission( YafContext context, ViewPermissions permission )
		{
			if ( permission == ViewPermissions.Everyone )
			{
				return true;
			}
			else if ( permission == ViewPermissions.RegisteredUsers )
			{
				return !context.IsGuest;
			}
			else
			{
				return context.IsAdmin;
			}
		}

		static public void HandleRequest( YafContext context, int permission )
		{
			HandleRequest( context, ( ViewPermissions )permission );
		}
		static public void HandleRequest( YafContext context, ViewPermissions permission )
		{
			if ( !CheckPermission( context, permission ) )
			{
				if ( permission == ViewPermissions.RegisteredUsers )
				{
					YAF.Classes.Utils.YafBuildLink.Redirect( YAF.Classes.Utils.ForumPages.login, "ReturnUrl={0}", General.GetSafeRawUrl() );
				}
				else
				{
					YafBuildLink.AccessDenied();
				}
			}
		}

		static public string EncodeMessage( string message )
		{
			if ( message.IndexOf( '<' ) >= 0 )
				return HttpUtility.HtmlEncode( message );

			return message;
		}


		/// <summary>
		/// Compares two messages.
		/// </summary>
		/// <param name="originalMessage">Original message text.</param>
		/// <param name="newMessage">New message text.</param>
		/// <returns>True if messages differ, false if they are identical.</returns>
		static public bool CompareMessage( Object originalMessage, Object newMessage )
		{
			return ( ( String )originalMessage != ( String )newMessage );
		}
	}
}
