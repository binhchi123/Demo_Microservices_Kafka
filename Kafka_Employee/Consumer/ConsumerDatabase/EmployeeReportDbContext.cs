﻿using ConsumerDatabase.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerDatabase
{
    public class EmployeeReportDbContext : DbContext
    {
        public EmployeeReportDbContext(DbContextOptions<EmployeeReportDbContext> options) : base(options) { }

        public DbSet<EmployeeReport> Reports { get; set; }
    }
}