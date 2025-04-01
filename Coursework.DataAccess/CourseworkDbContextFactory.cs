using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.DataAccess
{
    public class CourseworkDbContextFactory : IDesignTimeDbContextFactory<CourseworkDbContext>
    {
        public CourseworkDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CourseworkDbContext>();
            optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\user\\Documents\\CourseworkDb.mdf;Integrated Security=True;Connect Timeout=30");

            return new CourseworkDbContext(optionsBuilder.Options);
        }
    }
}
