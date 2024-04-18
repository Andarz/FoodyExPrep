using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoodyExPrep.Models
{
	public class AuthenticationResponse
	{
		[JsonPropertyName("username")]
		public string UserName { get; set; }

		[JsonPropertyName("password")]
		public string Password { get; set; }

		[JsonPropertyName("accessToken")]
		public string AccessToken { get; set; }
	}
}
