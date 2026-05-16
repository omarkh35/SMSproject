using BLL.EntitiesDTOS.User;
using BLL.Interfaces;
using BLL.Services;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class DepencyInjection
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
           
            return services;
        }


    }
}
