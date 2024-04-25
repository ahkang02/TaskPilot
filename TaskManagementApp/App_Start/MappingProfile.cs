using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementApp.DTO;
using TaskManagementApp.Models;

namespace TaskManagementApp.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            Mapper.CreateMap<ApplicationUser, UserDTO>();
            Mapper.CreateMap<UserDTO, ApplicationUser>();

        }

    }
}