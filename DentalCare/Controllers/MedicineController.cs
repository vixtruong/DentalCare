﻿using DentalCare.Models;
using DentalCare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace DentalCare.Controllers
{
    [Authorize]
    public class MedicineController : Controller
    {
        private readonly MedicineService _medicineService;
        private readonly MedicineTypeService _medicineTypeService;

        public MedicineController(MedicineService medicineService, MedicineTypeService medicineTypeService)
        {
            _medicineService = medicineService;
            _medicineTypeService = medicineTypeService;
        }

        [HttpGet]
        public IActionResult GetQuantityForMedicine(string medicineId)
        {
            var quantity = _medicineService.GetQuantityForMedicine(medicineId);
            return Json(new { quantity });
        }

        [Route("medicine/manage")]
        public IActionResult Index(string sortColumn, string sortDirection, string searchQuery, int? page = 1)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole.Contains("D"))
            {
                return NotFound();
            }

            ViewBag.MedicineTypes = _medicineTypeService.GetAll();
            var pageNumber = (page ?? 1);
            var pageSize = 10;
            var medicines = _medicineService.GetAll();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                medicines = medicines.Where(a => a.Id.Contains(searchQuery) ||
                                                 a.Name.ToString().Contains(searchQuery) ||
                                                 a.Medicinetype.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

                ViewBag.SearchQuery = searchQuery;
            }

            medicines = (sortColumn switch
            {
                "ID" => sortDirection == "desc" ? medicines.OrderByDescending(a => a.Id) : medicines.OrderBy(a => a.Id),
                "Type" => sortDirection == "desc" ? medicines.OrderByDescending(a => a.Medicinetype?.Name ?? "") : medicines.OrderBy(a => a.Medicinetype?.Name ?? ""),
                "Name" => sortDirection == "desc" ? medicines.OrderByDescending(a => a.Name) : medicines.OrderBy(a => a.Name),
                "Unit" => sortDirection == "desc" ? medicines.OrderByDescending(a => a.Unit) : medicines.OrderBy(a => a.Unit),
                "Quantity" => sortDirection == "desc" ? medicines.OrderByDescending(a => a.Quantity) : medicines.OrderBy(a => a.Quantity),
                "Price" => sortDirection == "desc" ? medicines.OrderByDescending(a => a.Price) : medicines.OrderBy(a => a.Price),
                _ => medicines.OrderBy(a => a.Id)
            }).ToList();

            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.NextSortDirection = sortDirection == "asc" ? "desc" : "asc";

            var pagedList = medicines.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        [Route("medicine/add-type")]
        [HttpGet]
        public IActionResult AddType()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole.Contains("D"))
            {
                return NotFound();
            }

            return View();
        }

        [Route("medicine/add-type")]
        [HttpPost]
        public IActionResult AddType(Medicinetype type)
        {
            Medicinetype newType = new Medicinetype
            {
                Id = _medicineTypeService.GenerateID(),
                Name = type.Name,
            };

            _medicineTypeService.Add(newType);
            return View("Index");
        }

        [Route("medicine/add")]
        [HttpGet]
        public IActionResult Add()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole.Contains("D"))
            {
                return NotFound();
            }

            ViewBag.Types = _medicineTypeService.GetAll();
            return View();
        }

        [Route("medicine/add")]
        [HttpPost]
        public IActionResult Add(Medicine medicine)
        {
            medicine.Id = _medicineService.GenerateID();
            _medicineService.Add(medicine);
            return RedirectToAction("Index");
        }

        [Route("medicine/edit/{id}")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole.Contains("D"))
            {
                return NotFound();
            }

            var medicine = _medicineService.Get(id);
            ViewBag.Types = _medicineTypeService.GetAll();
            return View(medicine);
        }

        [Route("medicine/edit/{id}")]
        [HttpPost]
        public IActionResult Edit(Medicine medicine)
        {
            _medicineService.Update(medicine);
            return RedirectToAction("Index", "Medicine");
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole.Contains("D"))
            {
                return NotFound();
            }

            try
            {
                _medicineService.Delete(id);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "This medicine relate to your business database! Can not delete";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
