using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Web.Http;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Mvc.Controllers.API;
using LinqToLdap.Examples.Mvc.Models;
using LinqToLdap.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace LinqToLdap.Examples.Mvc.Tests
{
    [TestClass]
    public class UsersControllerTest
    {
        private UsersController _usersController;
        private Mock<IDirectoryContext> _mockContext;

        [TestInitialize]
        public void SetUp()
        {
            _mockContext = new Mock<IDirectoryContext>();
            _usersController = new UsersController(_mockContext.Object);
        }

        [TestMethod]
        public void Get_NotFound_ThrowsNotFoundException()
        {
            //arrange
            var list = new List<User>();

            _mockContext.Setup(x => x.Query<User>(SearchScope.Subtree, null))
                .Returns(list.AsQueryable());

            //assert
            Executing.This(() => _usersController.Get("1"))
                .Should().Throw<HttpResponseException>().And.Exception.Response.StatusCode
                .Should().Be.EqualTo(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public void Get_Found_ReturnsUser()
        {
            //arrange
            var list = new List<User>
            {
                new User {UserId = "2"}
            };
            _mockContext.Setup(x => x.Query<User>(SearchScope.Subtree, null))
                .Returns(list.AsQueryable());

            //act
            var user = _usersController.Get("2");

            //assert
            user.Should().Be.EqualTo(list[0]);
        }

        [TestMethod]
        public void Get_Empty_Search_Returns_All_Results()
        {
            //arange
            var list = new List<User>
            {
                new User {DistinguishedName = "test", FirstName = "first", LastName = "last", UserId = "1"}
            };
            _mockContext.Setup(x => x.Query<User>(SearchScope.Subtree, null))
                .Returns(list.AsQueryable());

            //act
            var results = _usersController.Find("", false) as UserListItem[];

            //assert
            results.Should().Have.Count.EqualTo(1);
            results[0].DistinguishedName.Should().Be.EqualTo("test");
            results[0].Name.Should().Be.EqualTo("first last");
            results[0].UserId.Should().Be.EqualTo("1");
        }

        [TestMethod]
        public void Get_Custom_Search_Returns_Results()
        {
            //arange
            var expectedResults = new List<object>
            {
                new[] {new UserListItem {DistinguishedName = "test", Name = "test", UserId = "test"}}
            };
            var mockQuery = new MockQuery<User>(expectedResults);
            _mockContext.Setup(x => x.Query<User>(SearchScope.Subtree, null))
                .Returns(mockQuery);

            //act
            var results = _usersController.Find("blah blah", true) as IEnumerable<object>;

            //assert
            results.Satisfy(x => x.Count() == 1 && ((UserListItem)x.First()).DistinguishedName == "test");
            mockQuery.MockProvider.ExecutedExpressions[0].ToString()
                .Should()
                .Contain(
                    "FilterWith(\"(blah blah)\").Select(s => new UserListItem() {DistinguishedName = s.DistinguishedName, Name = Format(\"{0} {1}\", s.FirstName, s.LastName), PrimaryAffiliation = s.PrimaryAffiliation, UserId = s.UserId})");
        }

        [TestMethod]
        public void Get_Standard_Search_Two_Search_Terms_Returns_Correct_Results()
        {
            //arange
            var list = new List<User>
            {
                new User {DistinguishedName = "test 4", FirstName = "john x", LastName = "doe x", UserId = "1"},
                new User {DistinguishedName = "test 1", FirstName = "term", LastName = "last", UserId = "1"},
                new User {DistinguishedName = "test 2", FirstName = "first", LastName = "term", UserId = "1"},
                new User {DistinguishedName = "test 3", FirstName = "first", LastName = "last", UserId = "term"}
            };
            _mockContext.Setup(x => x.Query<User>(SearchScope.Subtree, null))
                .Returns(list.AsQueryable());

            //act
            var results = _usersController.Find("john doe", false) as UserListItem[];

            //assert
            results.Satisfy(x => x.Count() == 1 && x.First().DistinguishedName == "test 4");
        }

        [TestMethod]
        public void Get_Standard_Search_One_Search_Terms_Returns_Correct_Results()
        {
            //arange
            var list = new List<User>
            {
                new User {DistinguishedName = "test 4", FirstName = "john x", LastName = "doe x", UserId = "1"},
                new User {DistinguishedName = "test 1", FirstName = "term", LastName = "last", UserId = "1"},
                new User {DistinguishedName = "test 2", FirstName = "first", LastName = "term", UserId = "1"},
                new User {DistinguishedName = "test 3", FirstName = "first", LastName = "last", UserId = "term"}
            };
            _mockContext.Setup(x => x.Query<User>(SearchScope.Subtree, null))
                .Returns(list.AsQueryable());

            //act
            var results = _usersController.Find("term", false) as UserListItem[];

            //assert
            results.Satisfy(
                x => x.Count() == 3 &&
                     x.All(y =>
                         y.DistinguishedName == "test 1" || y.DistinguishedName == "test 2" ||
                         y.DistinguishedName == "test 3"));
        }
    }
}
