﻿using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.OrderRepository
{
    public class OrderDetailRepository(CompanyDBContext context) : CommonRepository<OrderDetail>(context), IOrderDetailRepository
    {
        private readonly CompanyDBContext _context  = context;

        public override async Task<IEnumerable<OrderDetail>> GetAsync(Expression<Func<OrderDetail, bool>> expression)
        {
            return await _context.OrderDetails.Include(e => e.Product).Where(expression).ToListAsync();
        }
    }
}