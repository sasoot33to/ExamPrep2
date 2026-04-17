using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using TheFoodySystemExamPrep.Models;




namespace TheFoodySystemExamPrep
{
    public class Tests
    {
        //   [TestFixture]

        private RestClient client;
        private static string foodID;

        private const string BaseUrl = "http://144.91.123.158:81";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI4MjlhN2EzNy02ZjM2LTRlN2EtYjk1Mi05ZDk3YzAxZTVhOGYiLCJpYXQiOiIwNC8xNy8yMDI2IDE2OjI2OjM3IiwiVXNlcklkIjoiNjU2MjAwNjItNTU2Ni00ODEzLWFlMmMtMDhkZTY4OGM0NGJjIiwiRW1haWwiOiJzYXNyQGV4cGVyZXAyLmNvbSIsIlVzZXJOYW1lIjoiU2FobzEyMyIsImV4cCI6MTc3NjQ2NDc5NywiaXNzIjoiRm9vZHlfQXBwX1NvZnRVbmkiLCJhdWQiOiJGb29keV9XZWJBUElfU29mdFVuaSJ9.PtJ84kJk3ZxQJ8UR8lJe9yN-FW-eOJIWs_rbmx1jVB4";

        private const string Userneme = "Saho123";
        private const string Password = "123123";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {

                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(Userneme, Password);
            }
            var options = new RestClientOptions(BaseUrl)

            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);

        }

        private string GetJwtToken(string userneme, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userneme, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }

        }




        [Order(1)]
        [Test]
        public void CreateNewFood_With_RequiredFields()
        {
            var createFood = new FoodDTO
            {
                Name = "Pizza Margarithe",
                Description = "Pizza with mozzarella",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(createFood);


            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            ;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));


            ApiResponseDTO redyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            foodID = redyResponse.FoodId;
        }

        [Order(2)]
        [Test]

        public void EditTitle_of_CreatedFood()
        {
            var createFood = new FoodDTO
            {
                Name = "Pizza Margarithe",
                Description = "Pizza with mozzarella",
                Url = ""
            };
            var request = new RestRequest($"/api/Food/Edit/{foodID}", Method.Patch);

            request.AddBody(new[]
            {
                new
                {
                    path ="/name",
                    op = "replace",
                    value ="Pizza Peperoni"
                }

            });

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }
        [Order(3)]
        [Test]
        public void GetAllGood_ReturnAllGoods()
        {


            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems, Is.Not.Null);
        }

        [Order(4)]
        [Test]
        public void DeleteEditedFood_WorksPoperly()
        {

            var request = new RestRequest($"/api/Food/Delete/{foodID}", Method.Delete);
            var response = this.client.Execute(request);


            var deltResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(deltResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateFoodWitoutReqireFields_ShouldReturnError()
        {
            var createFood = new FoodDTO
            {
                Name = "Pizza Margarithe",
                Description = "",

            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(createFood);


            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);



            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }
        [Order(5)]
        [Test]
        public void EditNonExistingFoosd_ShouldReturnError()

          
        {

            string ninExistingFoodID = "124324";
            var request = new RestRequest($"/api/Food/Edit/{ninExistingFoodID}", Method.Patch);

            request.AddBody(new[]
            {
                new
                {
                    path ="/name",
                    op = "replace",
                    value ="Pizza Peperoni"
                }

            });

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(editResponse.Msg, Is.EqualTo("No food revues..."));
        }
        [Order(6)]
        [Test]

        public void DeleteNinnExistingFood_ShouldReturnError()
        {

            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Delete/", Method.Delete);
            request.AddQueryParameter("foodId", nonExistingFoodId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

                Assert.That(editResponse.Msg, Is.EqualTo("No food revues..."));
            }
        }

        [OneTimeTearDown]

        public void TearDown()
        {
            this.client.Dispose();
        }
    }
}

//string nonExistingFoodId = "12345";
//RestRequest request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);
//var response = this.client.Execute(request);
//Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));


//var deltResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);


//Assert.That(deltResponse.Msg, Is.EqualTo("No food revues..."));