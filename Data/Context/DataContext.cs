﻿using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<VerificatioRequestEntity> VerificatioRequests { get; set;}
}
