using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SampleApi.ViewModels
{
    public class PaginationViewModel
    {
        [FromQuery]
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [FromQuery]
        [Range(1, 100)]
        public int Limit { get; set; } = 10;

        public int Skip
        {
            get
            {
                return (Page - 1) * Limit;
            }
        }
    }
}