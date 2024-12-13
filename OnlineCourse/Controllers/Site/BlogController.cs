﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCourse.Contexts;
using OnlineCourse.Controllers.Panel;
using OnlineCourse.Extensions;
using OnlineCourse.Services;

namespace OnlineCourse.Controllers.Site
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMinioService _minioService;
        public BlogController(ApplicationDbContext context, IMinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }
        [HttpGet("top-blogs")]
        public async Task<IActionResult> GetTopBlogs()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(c => c.Visit)
                .OrderByDescending(c => c.CreateDate)
                .Take(10)
                .Select(c => new
                {
                    c.Title,
                    c.Id,
                })
                .ToListAsync();

            return OkB(blogs);
        }

        [HttpGet]
        //Todo: Add Caching,Ratelimit
        public async Task<IActionResult> GetBlogs(PagedRequest pagedRequest)
        {
            var query = _context.Blogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(pagedRequest.Search))
            {
                query = query.Where(c => c.Title.Contains(pagedRequest.Search));
            }
            var blogs = query
                .OrderByDescending(c => c.CreateDate)
                .Skip((pagedRequest.PageNumber - 1) * pagedRequest.PageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Tags,
                    CreateDate = c.CreateDate.ToPersianDateTime(),
                    c.Title,
                    c.Content,
                    c.Visit,
                    _minioService.GetFileUrlAsync("ma-blog", c.ImageFileName).Result
                }).ToList();

            return OkB(new PagedResponse<object>
            {
                TotalCount = query.Count(),
                PageNumber = pagedRequest.PageNumber,
                PageSize = pagedRequest.PageSize,
                Result = blogs
            });
        }
        [HttpGet("{blogId}")]
        public async Task<IActionResult> GetBlog(int blogId)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == blogId);
            if (blog == null)
            {
                return NotFoundB("مقاله مورد نظر یافت نشد");
            }
            blog.Visit++;
            await _context.SaveChangesAsync();
            return OkB(new
            {
                blog.Id,
                blog.Tags,
                CreateDate = blog.CreateDate.ToPersianDateTime(),
                blog.Title,
                blog.Content,
                blog.Visit,
                _minioService.GetFileUrlAsync("ma-blog", blog.ImageFileName).Result
            });
        }
    }
}