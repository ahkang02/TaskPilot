using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskPilot.Application.Common.Interfaces;
using TaskPilot.Application.Services.Implementation;
using TaskPilot.Application.Services.Interface;
using TaskPilot.Domain.Entities;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class UserPermissionServiceTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private List<Features> _mockFeatures;
        private List<Permission> _mockPermissions;
        private List<ApplicationRole> _mockRole;
        private ClaimsIdentity mockUserClaims;
        private IUserPermissionService _userPermissionService;

        [SetUp]
        public void Setup()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            mockUserClaims = new ClaimsIdentity();
            mockUserClaims.AddClaim(new Claim(ClaimTypes.Role, "MockRole1"));

            _mockRole = new List<ApplicationRole>
            {
                new ApplicationRole { Id = "feb7cab8-4774-4618-b40f-d987d6294827", Name = "MockRole1"},
                                new ApplicationRole { Id = "feb7cab8-4774-4618-b40f-d987d6294826", Name = "Mock"},
                                new ApplicationRole { Id = "feb7cab8-4774-4618-b40f-d987d6294826", Name = "Mock3"},
                                new ApplicationRole { Id = "feb7cab8-4774-4618-b40f-d987d6294826", Name = "Mock4", Permissions = new List<Permission>()},
            };

            _mockFeatures = new List<Features>
            {
                new Features { Id = new Guid("03b4072d-a674-40b1-a159-bb9c24551d40"), Name = "Feature 1" },
                new Features { Id = new Guid("8af96401-8837-4f6a-b70f-ab49ef1be7f7"), Name = "Feature 2" }
            };

            _mockPermissions = new List<Permission>
            {
               new Permission { Id = new Guid("9a12a12f-7bb5-42de-b6ac-8d8c6375f926"), Name = "Permission1", Roles = new List<ApplicationRole> { _mockRole.Single(r => r.Name == "MockRole1") }, Features = _mockFeatures.Single(f => f.Name == "Feature 1"), FeaturesId = _mockFeatures.Single(f => f.Name == "Feature 1").Id },
               new Permission { Id = new Guid("9a12a12f-7bb5-42de-b6ac-8d8c6375f927"), Name = "Permission2", Roles = new List<ApplicationRole> { _mockRole.Single(r => r.Name == "Mock") }, Features = _mockFeatures.Single(f => f.Name == "Feature 1"), FeaturesId = _mockFeatures.Single(f => f.Name == "Feature 1").Id },
               new Permission { Id = new Guid("9a12a12f-7bb5-42de-b6ac-8d8c6375f928"), Name = "Permission3", Roles = new List<ApplicationRole> { _mockRole.Single(r => r.Name == "Mock3") }, Features = _mockFeatures.Single(f => f.Name == "Feature 1"), FeaturesId = _mockFeatures.Single(f => f.Name == "Feature 1").Id },
            };

            unitOfWorkMock.Setup(uow => uow.Permissions.GetAllInclude(null, "Features,Roles"))
    .Returns(_mockPermissions);

            unitOfWorkMock.Setup(uow => uow.Roles.GetAllInclude(r => r.Name == "MockRole1", "Permissions"))
    .Returns(_mockRole);



            _userPermissionService = new UserPermissionService(unitOfWorkMock.Object);
        }

        [Test]
        public void GetUserPermission_ReturnsUserPermissions()
        {
            // Act
            var result = _userPermissionService.GetUserPermission(mockUserClaims);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result, Does.Contain(_mockPermissions.Single(r => r.Name == "Permission1")));
        }

        [Test]
        public void GetUserPermissionWithNullUserClaims_ThrowsArgumentNullException()
        {
            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => _userPermissionService.GetUserPermission(null));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Value cannot be null. (Parameter 'userClaims')"));
        }
        

        //  TODO: Need To Think Of A Way To Do This Unit Test
        //    [Test]
        //public void GetUserPermissionWithUserClaims_ReturnEmptyList()
        //{
        //    // Arrange
        //    mockUserClaims = new ClaimsIdentity();
        //    mockUserClaims.AddClaim(new Claim(ClaimTypes.Role, "Mock"));

        //    // Act
        //    var result = _userPermissionService.GetUserPermission(mockUserClaims);

        //    // Assert
        //    Assert.That(result.Count(), Is.EqualTo(0));
        //}

    }
}
