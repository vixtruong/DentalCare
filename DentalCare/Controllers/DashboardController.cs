﻿using DentalCare.Models;
using DentalCare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly InvoiceService _invoiceService;
        private readonly MedicalExamService _medicalExamService;

        public DashboardController(InvoiceService invoiceService, MedicalExamService medicalExamService)
        {
            _invoiceService = invoiceService;
            _medicalExamService = medicalExamService;
        }

        [Route("dashboard")]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId.Contains("D"))
            {
                long uTotalSale = 0;
                long uTodaySale = 0;
                int uCustomerQuantity = 0;
                int uInvoiceQuantity = 0;

                var mesList = _medicalExamService.GetAll().Where(x => x.Doctorid == userId);

                foreach (var invoice in _invoiceService.GetAll().Where(x => mesList.Any(m => m.Id == x.Medicalexaminationid)))
                {
                    if (invoice.Date == DateTime.Today)
                    {
                        uTodaySale += invoice.Finaltotal;
                        uInvoiceQuantity += 1;
                    }

                    if (invoice.Date.Month == DateTime.Today.Month)
                    {
                        uTotalSale += invoice.Finaltotal;
                    }
                }

                foreach (var mes in mesList)
                {
                    if (mes.Date == DateTime.Today)
                    {
                        uCustomerQuantity += 1;
                    }
                }

                var uRevenue = new SaleRevenueViewModel
                {
                    TotalSale = uTotalSale,
                    TodaySale = uTodaySale,
                    CustomerQuantity = uCustomerQuantity,
                    InvoiceQuantity = uInvoiceQuantity
                };

                return View(uRevenue);
            }

            long totalSale = 0;
            long todaySale = 0;
            int customerQuantity = 0;
            int invoiceQuantity = 0;

            foreach (var invoice in _invoiceService.GetAll())
            {
                if (invoice.Date == DateTime.Today)
                {
                    todaySale += invoice.Finaltotal;
                    invoiceQuantity += 1;
                }

                if (invoice.Date.Month == DateTime.Today.Month)
                {
                    totalSale += invoice.Finaltotal;
                }
            }

            foreach (var mes in _medicalExamService.GetAll())
            {
                if (mes.Date == DateTime.Today)
                {
                    customerQuantity += 1;
                }
            }

            var revenue = new SaleRevenueViewModel
            {
                TotalSale = totalSale,
                TodaySale = todaySale,
                CustomerQuantity = customerQuantity,
                InvoiceQuantity = invoiceQuantity
            };

            return View(revenue);
        }

        [HttpGet]
        public IActionResult GetRevenuePreviousMonth()
        {
            int currentYear = DateTime.Today.Year;
            int preMonth = DateTime.Today.Month - 1;

            if (preMonth == 0)
            {
                preMonth = 12;
                currentYear -= 1;
            }

            int daysInPreMonth = DateTime.DaysInMonth(currentYear, preMonth);

            var invoices = _invoiceService.GetAll();
            var userId = HttpContext.Session.GetString("UserId");
            var doctorMes = _medicalExamService.GetAll().Where(x => x.Doctorid == userId);

            if (userId != null && userId.Contains("D"))
            {
                invoices = invoices.Where(x => doctorMes.Any(m => m.Id == x.Medicalexaminationid)).ToList();
            }

            Dictionary<int, int> revenue = new Dictionary<int, int>();

            for (int i = 1; i <= daysInPreMonth; i++)
            {
                int sum = 0;
                foreach (var invoice in invoices)
                {
                    if (invoice.Date.Year == currentYear && invoice.Date.Month == preMonth && invoice.Date.Day == i)
                    {
                        sum += invoice.Finaltotal;
                    }
                }

                revenue.Add(i, sum);
            }

            return Json(revenue);
        }

        [HttpGet]
        public IActionResult GetRevenueCurrentMonth()
        {
            int currentYear = DateTime.Today.Year;
            int currentMonth = DateTime.Today.Month;

            int daysInCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            Dictionary<int, int> revenue = new Dictionary<int, int>();

            var invoices = _invoiceService.GetAll();
            var userId = HttpContext.Session.GetString("UserId");
            var doctorMes = _medicalExamService.GetAll().Where(x => x.Doctorid == userId);

            if (userId != null && userId.Contains("D"))
            {
                invoices = invoices.Where(x => doctorMes.Any(m => m.Id == x.Medicalexaminationid)).ToList();
            }

            for (int i = 1; i <= daysInCurrentMonth; i++)
            {
                int sum = 0;
                foreach (var invoice in invoices)
                {
                    if (invoice.Date.Year == currentYear && invoice.Date.Month == currentMonth && invoice.Date.Day == i)
                    {
                        sum += invoice.Finaltotal;
                    }
                }

                revenue.Add(i, sum);
            }

            return Json(revenue);
        }

    }
}
