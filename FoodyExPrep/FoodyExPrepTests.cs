using FoodyExPrep.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace FoodyExPrep
{
	[TestFixture]
	public class FoodyExPrepTests
	{
		private RestClient client;
		private static string foodId;

		[OneTimeSetUp]
		public void Setup()
		{
			string jwtToken = GetJwtToken("andarz", "test123");

			var options = new RestClientOptions("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86")
			{
				Authenticator = new JwtAuthenticator(jwtToken)
			};

			this.client = new RestClient(options);
		}

		private string GetJwtToken(string username, string password)
		{
			var tempClient = new RestClient("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86");
			var request = new RestRequest("/api/User/Authentication", Method.Post);
			request.AddJsonBody(new AuthenticationRequest
			{ UserName = username,
			  Password = password
			});

			var response = tempClient.Execute(request);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var content = JsonSerializer.Deserialize<AuthenticationResponse>(response.Content);
				var token = content.AccessToken;
				if (string.IsNullOrWhiteSpace(token))
				{
					throw new InvalidOperationException("The JWT token is null or empty.");
				}
				return token;
			}
			else
			{
				throw new InvalidOperationException($"Authentication failed: {response.StatusCode}, {response.Content}");
			}
		}

		[Order(1)]
		[Test]
		public void CreateNewFoodWithTheRequiredFields_ShouldCreateAFood()
		{
			// Arrange
			var newFood = new FoodDto
			{
				Name = "New Test Food",
				Description = "New Test Food Description"
			};

			var request = new RestRequest("/api/Food/Create", Method.Post);
			request.AddJsonBody(newFood);

			// Act
			var response = this.client.Post(request);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

			var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
			foodId = responseData.FoodId;
		}

		[Order(2)]
		[Test]
		public void EditFoodWithNewTitle_ShouldSucceed()
		{
			// Arrange
			var request = new RestRequest($"/api/Food/Edit/{foodId}");

			request.AddJsonBody(new[]
			{
				new
				{
					path = "/name",
					op = "replace",
					value = "Edited Test Food Title"
				}
			});

			// Act
			var response = this.client.Patch(request);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(responseData.Message, Is.EqualTo("Successfully edited"));
		}

		[Order(3)]
		[Test]

		public void GetAllFoodShouldReturnAnArrayOfFoods()
		{
			// Arrange
			var request = new RestRequest("/api/Food/All");

			// Act
			var response = this.client.Get(request);
			var responseData = JsonSerializer.Deserialize<List<ApiResponseDto>>(response.Content);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(responseData.Count, Is.GreaterThan(0));
		}

		[Order(4)]
		[Test]

		public void DeleteFoodWithValidDataShouldSucceed()
		{
			// Arrange
			var request = new RestRequest($"/api/Food/Delete/{foodId}");

			// Act
			var response = this.client.Delete(request);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

			var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(responseData.Message, Is.EqualTo("Deleted successfully!"));
		}

		[Order(5)]
		[Test]

		public void CreateAFoodWithoutRequiredData_ShouldFail()
		{
			// Arrange
			var request = new RestRequest("/api/Food/Create", Method.Post);
			request.AddJsonBody(new { });

			// Act
			var response = this.client.Execute(request);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
		}

		[Order(6)]
		[Test]

		public void EditAnNonExistingFood_ShouldFail()
		{
			// Arrange
			var request = new RestRequest($"/api/Food/Edit/77777");

			request.AddJsonBody(new[]
			{
				new
				{
					path = "/name",
					op = "replace",
					value = "Edited Test Food Title"
				}
			});

			// Act
			var response = this.client.Patch(request);
			var responseData = JsonSerializer.Deserialize<ApiResponseDto> (response.Content);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
			Assert.That(responseData.Message, Is.EqualTo("No food revues..."));
		}

		[Order(7)]
		[Test]

		public void DeleteNonExistingFoodShouldFail()
		{
			// Arrange
			var request = new RestRequest($"/api/Food/Delete/77777", Method.Delete);

			// Act
			var response = this.client.Execute(request);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

			var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

			Assert.That(responseData.Message, Is.EqualTo("Unable to delete this food revue!"));
		}
	}
}