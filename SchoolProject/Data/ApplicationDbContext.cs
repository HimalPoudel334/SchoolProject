using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //scared already
        public DbSet<Ngo> Ngos { get; set; }
        public DbSet<Donor> Donors { get; set; }

        //regular model
        public DbSet<Medicine> Medicines { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }
        
        //scary models
        public DbSet<Donation> Donations { get; set; }
        
        public DbSet<Request> Requests { get; set; }
    }
}
