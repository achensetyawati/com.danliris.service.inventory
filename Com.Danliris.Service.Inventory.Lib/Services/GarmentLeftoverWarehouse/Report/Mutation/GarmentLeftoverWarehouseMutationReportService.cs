﻿using Com.Danliris.Service.Inventory.Lib.Helpers;
using Com.Danliris.Service.Inventory.Lib.ViewModels.GarmentLeftoverWarehouse.Report.Mutation;
using Com.Moonlay.NetCore.Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;

namespace Com.Danliris.Service.Inventory.Lib.Services.GarmentLeftoverWarehouse.Report.Mutation
{
    public class GarmentLeftoverWarehouseMutationReportService : IGarmentLeftoverWarehouseMutationReportService
    {
        private const string UserAgent = "GarmentLeftoverWarehouseMutationReportService";

        private InventoryDbContext DbContext;

        private readonly IServiceProvider ServiceProvider;
        private readonly IIdentityService IdentityService;

        public GarmentLeftoverWarehouseMutationReportService(InventoryDbContext dbContext, IServiceProvider serviceProvider)
        {
            DbContext = dbContext;
            ServiceProvider = serviceProvider;
            IdentityService = (IIdentityService)serviceProvider.GetService(typeof(IIdentityService));
        }

        public Tuple<List<GarmentLeftoverWarehouseMutationReportViewModel>, int> GetMutation(DateTime? dateFrom, DateTime? dateTo, int page, int size)
        {
            var Query = GetReportQuery(dateFrom, dateTo);

            //Pageable<GarmentLeftoverWarehouseMutationReportViewModel> pageable = new Pageable<GarmentLeftoverWarehouseMutationReportViewModel>(Query, page - 1, size);
            //List<GarmentLeftoverWarehouseMutationReportViewModel> Data = pageable.Data.ToList<GarmentLeftoverWarehouseMutationReportViewModel>();

            int TotalData = Query.Count();

            return Tuple.Create(Query, TotalData);
        }

        public List<GarmentLeftoverWarehouseMutationReportViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo)
        {
            DateTimeOffset DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTimeOffset)dateFrom;
            DateTimeOffset DateTo = dateTo == null ? DateTime.Now : (DateTimeOffset)dateTo;

            var BalanceDate = DbContext.GarmentLeftoverWarehouseBalanceStocks.OrderByDescending(x => x.BalanceStockDate).Select(x => x.BalanceStockDate).FirstOrDefault();


            var BalanceStock = (from a in DbContext.GarmentLeftoverWarehouseBalanceStocks
                                join b in DbContext.GarmentLeftoverWarehouseBalanceStocksItems on a.Id equals b.BalanceStockId
                                where a._IsDeleted == false && b._IsDeleted == false && a.TypeOfGoods == "BARANG JADI"
                                select new GarmentLeftoverWarehouseMutationReportViewModel
                                {
                                    ClassificationCode = "RJ001",
                                    ClassificationName = "Barang Jadi Reject",
                                    SaldoAwal = b.Quantity,
                                    Pemasukan = 0,
                                    Pengeluaran = 0,
                                    Penyesuaian = 0,
                                    Selisih = 0,
                                    SaldoAkhir = 0,
                                    StockOpname = 0,
                                    UnitQtyName = "PCS"
                                }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                {
                                    ClassificationCode = key.ClassificationCode,
                                    ClassificationName = key.ClassificationName,
                                    SaldoAwal = group.Sum(x => x.SaldoAwal),
                                    Pemasukan = group.Sum(x => x.Pemasukan),
                                    Pengeluaran = group.Sum(x => x.Pengeluaran),
                                    Penyesuaian = group.Sum(x => x.Penyesuaian),
                                    Selisih = group.Sum(x => x.Selisih),
                                    SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                    StockOpname = group.Sum(x => x.StockOpname),
                                    UnitQtyName = key.UnitQtyName

                                });

            var SAReceiptBarangJadi = (from a in DbContext.GarmentLeftoverWarehouseReceiptFinishedGoods
                                       join b in DbContext.GarmentLeftoverWarehouseReceiptFinishedGoodItems on a.Id equals b.FinishedGoodReceiptId
                                       where a._IsDeleted == false && b._IsDeleted == false
                                       && a.ReceiptDate > BalanceDate
                                       && a.ReceiptDate < DateFrom
                                       select new GarmentLeftoverWarehouseMutationReportViewModel
                                       {
                                           ClassificationCode = "RJ001",
                                           ClassificationName = "Barang Jadi Reject",
                                           SaldoAwal = b.Quantity,
                                           Pemasukan = 0,
                                           Pengeluaran = 0,
                                           Penyesuaian = 0,
                                           Selisih = 0,
                                           SaldoAkhir = 0,
                                           StockOpname = 0,
                                           UnitQtyName = "PCS"
                                       }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                       {
                                           ClassificationCode = key.ClassificationCode,
                                           ClassificationName = key.ClassificationName,
                                           SaldoAwal = group.Sum(x => x.SaldoAwal),
                                           Pemasukan = group.Sum(x => x.Pemasukan),
                                           Pengeluaran = group.Sum(x => x.Pengeluaran),
                                           Penyesuaian = group.Sum(x => x.Penyesuaian),
                                           Selisih = group.Sum(x => x.Selisih),
                                           SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                           StockOpname = group.Sum(x => x.StockOpname),
                                           UnitQtyName = key.UnitQtyName

                                       });

            //var SAReceiptAval = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
            //                     join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
            //                     where a._IsDeleted == false && b._IsDeleted == false
            //                     && a.ReceiptDate > BalanceDate
            //                     && a.ReceiptDate < DateFrom
            //                     && a.AvalType == "AVAL FABRIC"
            //                     select new GarmentLeftoverWarehouseMutationReportViewModel
            //                     {
            //                         ClassificationCode = "AV001" ,
            //                         ClassificationName = "Aval Besar",
            //                         SaldoAwal = b.Quantity,
            //                         Pemasukan = 0,
            //                         Pengeluaran = 0,
            //                         Penyesuaian = 0,
            //                         Selisih = 0,
            //                         SaldoAkhir = 0,
            //                         StockOpname = 0,
            //                         UnitQtyName = b.UomUnit
            //                     }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
            //                     {
            //                         ClassificationCode = key.ClassificationCode,
            //                         ClassificationName = key.ClassificationName,
            //                         SaldoAwal = group.Sum(x => x.SaldoAwal),
            //                         Pemasukan = group.Sum(x => x.Pemasukan),
            //                         Pengeluaran = group.Sum(x => x.Pengeluaran),
            //                         Penyesuaian = group.Sum(x => x.Penyesuaian),
            //                         Selisih = group.Sum(x => x.Selisih),
            //                         SaldoAkhir = group.Sum(x => x.SaldoAkhir),
            //                         StockOpname = group.Sum(x => x.StockOpname),
            //                         UnitQtyName = key.UnitQtyName

            //                     });
            var SaldoAwalReceiptAval = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
                                 join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
                                 where a._IsDeleted == false && b._IsDeleted == false
                                 && a.ReceiptDate > BalanceDate
                                 && a.ReceiptDate < DateFrom
                                 && a.AvalType == "AVAL FABRIC"
                                 select new 
                                 //GarmentLeftoverWarehouseMutationReportViewModel
                                 {
                                     AvalReceiptNo = a.AvalReceiptNo,
                                     ClassificationCode = "AV001",
                                     ClassificationName = "Aval Besar",
                                     //SaldoAwal = b.Quantity,
                                     SaldoAwal = a.TotalAval,
                                     Pemasukan = 0,
                                     Pengeluaran = 0,
                                     Penyesuaian = 0,
                                     Selisih = 0,
                                     SaldoAkhir = 0,
                                     StockOpname = 0,
                                     //UnitQtyName = b.UomUnit
                                     UnitQtyName = "KG"

                                 }).Distinct();

            var SAReceiptAval = SaldoAwalReceiptAval.Select(x => new GarmentLeftoverWarehouseMutationReportViewModel
            {
                //AvalReceiptNo = a.AvalReceiptNo,
                ClassificationCode = "AV001",
                ClassificationName = "Aval Besar",
                //SaldoAwal = b.Quantity,
                SaldoAwal = x.SaldoAwal,
                Pemasukan = 0,
                Pengeluaran = 0,
                Penyesuaian = 0,
                Selisih = 0,
                SaldoAkhir = 0,
                StockOpname = 0,
                UnitQtyName = x.UnitQtyName
            }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
            {
                ClassificationCode = key.ClassificationCode,
                ClassificationName = key.ClassificationName,
                SaldoAwal = group.Sum(x => x.SaldoAwal),
                Pemasukan = group.Sum(x => x.Pemasukan),
                Pengeluaran = group.Sum(x => x.Pengeluaran),
                Penyesuaian = group.Sum(x => x.Penyesuaian),
                Selisih = group.Sum(x => x.Selisih),
                SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                StockOpname = group.Sum(x => x.StockOpname),
                UnitQtyName = key.UnitQtyName

            });

            var SAReceiptAvalPen = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
                                 join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
                                 where a._IsDeleted == false && b._IsDeleted == false
                                 && a.ReceiptDate > BalanceDate
                                 && a.ReceiptDate < DateFrom
                                 && a.AvalType == "AVAL BAHAN PENOLONG"
                                    select new GarmentLeftoverWarehouseMutationReportViewModel
                                 {
                                     ClassificationCode = "AV004",
                                     ClassificationName = "Aval Bahan Penolong",
                                     Productname = b.ProductName,
                                     SaldoAwal = b.Quantity,
                                     Pemasukan = 0,
                                     Pengeluaran = 0,
                                     Penyesuaian = 0,
                                     Selisih = 0,
                                     SaldoAkhir = 0,
                                     StockOpname = 0,
                                     UnitQtyName = b.UomUnit
                                 }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.Productname, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                 {
                                     ClassificationCode = key.ClassificationCode,
                                     ClassificationName = key.ClassificationName,
                                     Productname = key.Productname,
                                     SaldoAwal = group.Sum(x => x.SaldoAwal),
                                     Pemasukan = group.Sum(x => x.Pemasukan),
                                     Pengeluaran = group.Sum(x => x.Pengeluaran),
                                     Penyesuaian = group.Sum(x => x.Penyesuaian),
                                     Selisih = group.Sum(x => x.Selisih),
                                     SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                     StockOpname = group.Sum(x => x.StockOpname),
                                     UnitQtyName = key.UnitQtyName

                                 });

            var SAReceiptAvalKomponen = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
                                             // join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
                                         where a._IsDeleted == false
                                         //&& b._IsDeleted == false
                                         && a.ReceiptDate > BalanceDate
                                         && a.ReceiptDate < DateFrom
                                         && a.AvalType == "AVAL KOMPONEN"
                                         select new
                                         {
                                             ClassificationCode = "AV002",
                                             ClassificationName = "Aval Komponen",
                                             SaldoAwal = a.TotalAval,
                                             Pemasukan = 0,
                                             Pengeluaran = 0,
                                             Penyesuaian = 0,
                                             Selisih = 0,
                                             SaldoAkhir = 0,
                                             StockOpname = 0,
                                             UnitQtyName = "KG"
                                         }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                         {
                                             ClassificationCode = key.ClassificationCode,
                                             ClassificationName = key.ClassificationName,
                                             SaldoAwal = group.Sum(x => x.SaldoAwal),
                                             Pemasukan = group.Sum(x => x.Pemasukan),
                                             Pengeluaran = group.Sum(x => x.Pengeluaran),
                                             Penyesuaian = group.Sum(x => x.Penyesuaian),
                                             Selisih = group.Sum(x => x.Selisih),
                                             SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                             StockOpname = group.Sum(x => x.StockOpname),
                                             UnitQtyName = group.First().UnitQtyName

                                         });



            var SAExpendBarangJadi = (from a in DbContext.GarmentLeftoverWarehouseExpenditureFinishedGoods
                                      join b in DbContext.GarmentLeftoverWarehouseExpenditureFinishedGoodItems on a.Id equals b.FinishedGoodExpenditureId
                                      where a._IsDeleted == false && b._IsDeleted == false
                                      && a.ExpenditureDate > BalanceDate
                                      && a.ExpenditureDate < DateFrom
                                      select new GarmentLeftoverWarehouseMutationReportViewModel
                                      {
                                          ClassificationCode = "RJ001",
                                          ClassificationName = "Barang Jadi Reject",
                                          SaldoAwal = b.ExpenditureQuantity * (-1),
                                          Pemasukan = 0,
                                          Pengeluaran = 0,
                                          Penyesuaian = 0,
                                          Selisih = 0,
                                          SaldoAkhir = 0,
                                          StockOpname = 0,
                                          UnitQtyName = "PCS"
                                      }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                      {
                                          ClassificationCode = key.ClassificationCode,
                                          ClassificationName = key.ClassificationName,
                                          SaldoAwal = group.Sum(x => x.SaldoAwal),
                                          Pemasukan = group.Sum(x => x.Pemasukan),
                                          Pengeluaran = group.Sum(x => x.Pengeluaran),
                                          Penyesuaian = group.Sum(x => x.Penyesuaian),
                                          Selisih = group.Sum(x => x.Selisih),
                                          SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                          StockOpname = group.Sum(x => x.StockOpname),
                                          UnitQtyName = key.UnitQtyName

                                      });

            var SAExpendAval = (from a in DbContext.GarmentLeftoverWarehouseExpenditureAvals
                                join b in DbContext.GarmentLeftoverWarehouseExpenditureAvalItems on a.Id equals b.AvalExpenditureId
                                where a._IsDeleted == false && b._IsDeleted == false
                                && a.ExpenditureDate > BalanceDate
                                && a.ExpenditureDate < DateFrom
                                && a.AvalType == "AVAL FABRIC"
                                select new GarmentLeftoverWarehouseMutationReportViewModel
                                {
                                    ClassificationCode = "AV001"  ,
                                    ClassificationName = "Aval Besar" ,
                                    SaldoAwal = b.Quantity * (-1),
                                    Pemasukan = 0,
                                    Pengeluaran = 0,
                                    Penyesuaian = 0,
                                    Selisih = 0,
                                    SaldoAkhir = 0,
                                    StockOpname = 0,
                                    //UnitQtyName = b.UomUnit
                                    UnitQtyName = "KG"
                                })
                                //.GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                //{
                                //    ClassificationCode = key.ClassificationCode,
                                //    ClassificationName = key.ClassificationName,
                                //    SaldoAwal = group.Sum(x => x.SaldoAwal),
                                //    Pemasukan = group.Sum(x => x.Pemasukan),
                                //    Pengeluaran = group.Sum(x => x.Pengeluaran),
                                //    Penyesuaian = group.Sum(x => x.Penyesuaian),
                                //    Selisih = group.Sum(x => x.Selisih),
                                //    SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                //    StockOpname = group.Sum(x => x.StockOpname),
                                //    UnitQtyName = key.UnitQtyName

                                //})
                                ;

            var SAExpendAvalKompo = (from a in DbContext.GarmentLeftoverWarehouseExpenditureAvals
                                     join b in DbContext.GarmentLeftoverWarehouseExpenditureAvalItems on a.Id equals b.AvalExpenditureId
                                     where a._IsDeleted == false && b._IsDeleted == false
                                     && a.ExpenditureDate > BalanceDate
                                     && a.ExpenditureDate < DateFrom
                                     && a.AvalType == "AVAL KOMPONEN"
                                     select new GarmentLeftoverWarehouseMutationReportViewModel
                                     {
                                         ClassificationCode = "AV002",
                                         ClassificationName = "Aval Komponen",
                                         SaldoAwal = b.Quantity * (-1),
                                         Pemasukan = 0,
                                         Pengeluaran = 0,
                                         Penyesuaian = 0,
                                         Selisih = 0,
                                         SaldoAkhir = 0,
                                         StockOpname = 0,
                                         UnitQtyName = b.UomUnit
                                     });


            var SAExpendAvalPen = (from a in DbContext.GarmentLeftoverWarehouseExpenditureAvals
                                join b in DbContext.GarmentLeftoverWarehouseExpenditureAvalItems on a.Id equals b.AvalExpenditureId
                                where a._IsDeleted == false && b._IsDeleted == false
                                && a.ExpenditureDate > BalanceDate
                                && a.ExpenditureDate < DateFrom
                                && a.AvalType == "AVAL BAHAN PENOLONG"
                                select new GarmentLeftoverWarehouseMutationReportViewModel
                                {
                                    ClassificationCode = "AV004",
                                    ClassificationName = "Aval Bahan Penolong",
                                    Productname = b.ProductName,
                                    SaldoAwal = b.Quantity * (-1),
                                    Pemasukan = 0,
                                    Pengeluaran = 0,
                                    Penyesuaian = 0,
                                    Selisih = 0,
                                    SaldoAkhir = 0,
                                    StockOpname = 0,
                                    UnitQtyName = b.UomUnit
                                });

            


            var SAwal = BalanceStock.Concat(SAReceiptBarangJadi).Concat(SAReceiptAval).Concat(SAReceiptAvalPen).Concat(SAReceiptAvalKomponen).Concat(SAExpendAval).Concat(SAExpendAvalPen).Concat(SAExpendBarangJadi).Concat(SAExpendAvalKompo).AsEnumerable();
            //var SAwal = BalanceStock.Concat(SAReceiptBarangJadi).Concat(SAReceiptAval).Concat(SAReceiptAvalPen).Concat(SAExpendAval).Concat(SAExpendAvalPen).Concat(SAExpendBarangJadi).AsEnumerable();
            var SaldoAwal = SAwal.GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.Productname, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
            {
                ClassificationCode = key.ClassificationCode,
                ClassificationName = key.ClassificationName,
                Productname = key.Productname,
                SaldoAwal = group.Sum(x => x.SaldoAwal),
                Pemasukan = group.Sum(x => x.Pemasukan),
                Pengeluaran = group.Sum(x => x.Pengeluaran),
                Penyesuaian = group.Sum(x => x.Penyesuaian),
                Selisih = group.Sum(x => x.Selisih),
                SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                StockOpname = group.Sum(x => x.StockOpname),
                UnitQtyName = key.UnitQtyName

            }).ToList();

            var FilteredReceiptBarangJadi = (from a in DbContext.GarmentLeftoverWarehouseReceiptFinishedGoods
                                             join b in DbContext.GarmentLeftoverWarehouseReceiptFinishedGoodItems on a.Id equals b.FinishedGoodReceiptId
                                             where a._IsDeleted == false && b._IsDeleted == false
                                             && a.ReceiptDate >= DateFrom
                                             && a.ReceiptDate <= DateTo
                                             select new GarmentLeftoverWarehouseMutationReportViewModel
                                             {
                                                 ClassificationCode = "RJ001",
                                                 ClassificationName = "Barang Jadi Reject",
                                                 SaldoAwal = 0,
                                                 Pemasukan = b.Quantity,
                                                 Pengeluaran = 0,
                                                 Penyesuaian = 0,
                                                 Selisih = 0,
                                                 SaldoAkhir = 0,
                                                 StockOpname = 0,
                                                 UnitQtyName = "PCS"
                                             }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                             {
                                                 ClassificationCode = key.ClassificationCode,
                                                 ClassificationName = key.ClassificationName,
                                                 SaldoAwal = group.Sum(x => x.SaldoAwal),
                                                 Pemasukan = group.Sum(x => x.Pemasukan),
                                                 Pengeluaran = group.Sum(x => x.Pengeluaran),
                                                 Penyesuaian = group.Sum(x => x.Penyesuaian),
                                                 Selisih = group.Sum(x => x.Selisih),
                                                 SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                                 StockOpname = group.Sum(x => x.StockOpname),
                                                 UnitQtyName = key.UnitQtyName

                                             });

            var FilteredReceiptAvalPen = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
                                       join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
                                       where a._IsDeleted == false && b._IsDeleted == false
                                       && a.ReceiptDate >= DateFrom
                                       && a.ReceiptDate <= DateTo
                                       && a.AvalType == "AVAL BAHAN PENOLONG"
                                          select new GarmentLeftoverWarehouseMutationReportViewModel
                                       {
                                           ClassificationCode = "AV004",
                                           ClassificationName =  "Aval Bahan Penolong",
                                           Productname = b.ProductName,
                                           SaldoAwal = 0,
                                           Pemasukan = b.Quantity,
                                           Pengeluaran = 0,
                                           Penyesuaian = 0,
                                           Selisih = 0,
                                           SaldoAkhir = 0,
                                           StockOpname = 0,
                                           UnitQtyName = b.UomUnit
                                       }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.Productname, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                       {
                                           ClassificationCode = key.ClassificationCode,
                                           ClassificationName = key.ClassificationName,
                                           Productname = key.Productname,
                                           SaldoAwal = group.Sum(x => x.SaldoAwal),
                                           Pemasukan = group.Sum(x => x.Pemasukan),
                                           Pengeluaran = group.Sum(x => x.Pengeluaran),
                                           Penyesuaian = group.Sum(x => x.Penyesuaian),
                                           Selisih = group.Sum(x => x.Selisih),
                                           SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                           StockOpname = group.Sum(x => x.StockOpname),
                                           UnitQtyName = key.UnitQtyName

                                       });

            //var FilteredReceiptAval = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
            //                           join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
            //                           where a._IsDeleted == false && b._IsDeleted == false
            //                           && a.ReceiptDate >= DateFrom
            //                           && a.ReceiptDate <= DateTo
            //                           && a.AvalType == "AVAL FABRIC"
            //                           select new 
            //                           //GarmentLeftoverWarehouseMutationReportViewModel
            //                           {
            //                               AvalReceiptNo = a.AvalReceiptNo,
            //                               ClassificationCode =  "AV001", 
            //                               ClassificationName =  "Aval Besar",
            //                               SaldoAwal = 0,
            //                               //Pemasukan = b.Quantity,
            //                               Pemasukan = a.TotalAval,
            //                               Pengeluaran = 0,
            //                               Penyesuaian = 0,
            //                               Selisih = 0,
            //                               SaldoAkhir = 0,
            //                               StockOpname = 0,
            //                               UnitQtyName = b.UomUnit
            //                           }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName,x.AvalReceiptNo,x.Pemasukan }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
            //                           {
            //                               ClassificationCode = key.ClassificationCode,
            //                               ClassificationName = key.ClassificationName,
            //                               SaldoAwal = group.Sum(x => x.SaldoAwal),
            //                               Pemasukan = key.Pemasukan,
            //                               Pengeluaran = group.Sum(x => x.Pengeluaran),
            //                               Penyesuaian = group.Sum(x => x.Penyesuaian),
            //                               Selisih = group.Sum(x => x.Selisih),
            //                               SaldoAkhir = group.Sum(x => x.SaldoAkhir),
            //                               StockOpname = group.Sum(x => x.StockOpname),
            //                               UnitQtyName = key.UnitQtyName

            //                           });

            var ReceiptAvalFabric = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
                                       join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
                                       where a._IsDeleted == false && b._IsDeleted == false
                                       && a.ReceiptDate >= DateFrom
                                       && a.ReceiptDate <= DateTo
                                       && a.AvalType == "AVAL FABRIC"
                                       select new
                                       //GarmentLeftoverWarehouseMutationReportViewModel
                                       {
                                           AvalReceiptNo = a.AvalReceiptNo,
                                           ClassificationCode = "AV001",
                                           ClassificationName = "Aval Besar",
                                           SaldoAwal = 0,
                                           //Pemasukan = b.Quantity,
                                           Pemasukan = a.TotalAval,
                                           Pengeluaran = 0,
                                           Penyesuaian = 0,
                                           Selisih = 0,
                                           SaldoAkhir = 0,
                                           StockOpname = 0,
                                           //UnitQtyName = b.UomUnit
                                           UnitQtyName = "KG"
                                       }).Distinct();

            var FilteredReceiptAval = ReceiptAvalFabric.Select(x => new GarmentLeftoverWarehouseMutationReportViewModel
            {
                ClassificationCode = x.ClassificationCode,
                ClassificationName = x.ClassificationName,
                SaldoAwal = 0,
                //Pemasukan = b.Quantity,
                Pemasukan = x.Pemasukan,
                Pengeluaran = 0,
                Penyesuaian = 0,
                Selisih = 0,
                SaldoAkhir = 0,
                StockOpname = 0,
                UnitQtyName = x.UnitQtyName
            }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName,}, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
            {
                //AvalReceiptNo = key.AvalReceiptNo,
                ClassificationCode = key.ClassificationCode,
                ClassificationName = key.ClassificationName,
                SaldoAwal = group.Sum(x => x.SaldoAwal),
                Pemasukan = group.Sum(x => x.Pemasukan),
                Pengeluaran = group.Sum(x => x.Pengeluaran),
                Penyesuaian = group.Sum(x => x.Penyesuaian),
                Selisih = group.Sum(x => x.Selisih),
                SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                StockOpname = group.Sum(x => x.StockOpname),
                UnitQtyName = key.UnitQtyName
            });

            var FilteredReceiptAvalKomponen = (from a in DbContext.GarmentLeftoverWarehouseReceiptAvals
                                                   //join b in DbContext.GarmentLeftoverWarehouseReceiptAvalItems on a.Id equals b.AvalReceiptId
                                               where a._IsDeleted == false
                                               //&& b._IsDeleted == false
                                               && a.ReceiptDate >= DateFrom
                                               && a.ReceiptDate <= DateTo
                                               && a.AvalType == "AVAL KOMPONEN"
                                               select new GarmentLeftoverWarehouseMutationReportViewModel
                                               {
                                                   ClassificationCode = "AV002",
                                                   ClassificationName = "Aval Komponen",
                                                   SaldoAwal = 0,
                                                   Pemasukan = a.TotalAval,
                                                   Pengeluaran = 0,
                                                   Penyesuaian = 0,
                                                   Selisih = 0,
                                                   SaldoAkhir = 0,
                                                   StockOpname = 0,
                                                   UnitQtyName = "KG"
                                               }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                               {
                                                   ClassificationCode = key.ClassificationCode,
                                                   ClassificationName = key.ClassificationName,
                                                   SaldoAwal = group.Sum(x => x.SaldoAwal),
                                                   Pemasukan = group.Sum(x => x.Pemasukan),
                                                   Pengeluaran = group.Sum(x => x.Pengeluaran),
                                                   Penyesuaian = group.Sum(x => x.Penyesuaian),
                                                   Selisih = group.Sum(x => x.Selisih),
                                                   SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                                   StockOpname = group.Sum(x => x.StockOpname),
                                                   UnitQtyName = group.First().UnitQtyName

                                               });

            var FilteredExpendBarangJadi = (from a in DbContext.GarmentLeftoverWarehouseExpenditureFinishedGoods
                                            join b in DbContext.GarmentLeftoverWarehouseExpenditureFinishedGoodItems on a.Id equals b.FinishedGoodExpenditureId
                                            where a._IsDeleted == false && b._IsDeleted == false
                                            && a.ExpenditureDate >= DateFrom
                                            && a.ExpenditureDate <= DateTo
                                            select new GarmentLeftoverWarehouseMutationReportViewModel
                                            {
                                                ClassificationCode = "RJ001",
                                                ClassificationName = "Barang Jadi Reject",
                                                SaldoAwal = 0,
                                                Pemasukan = 0,
                                                Pengeluaran = b.ExpenditureQuantity,
                                                Penyesuaian = 0,
                                                Selisih = 0,
                                                SaldoAkhir = 0,
                                                StockOpname = 0,
                                                UnitQtyName = "PCS"
                                            }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                            {
                                                ClassificationCode = key.ClassificationCode,
                                                ClassificationName = key.ClassificationName,
                                                SaldoAwal = group.Sum(x => x.SaldoAwal),
                                                Pemasukan = group.Sum(x => x.Pemasukan),
                                                Pengeluaran = group.Sum(x => x.Pengeluaran),
                                                Penyesuaian = group.Sum(x => x.Penyesuaian),
                                                Selisih = group.Sum(x => x.Selisih),
                                                SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                                StockOpname = group.Sum(x => x.StockOpname),
                                                UnitQtyName = key.UnitQtyName

                                            });

            var FilteredExpendAval = (from a in DbContext.GarmentLeftoverWarehouseExpenditureAvals
                                      join b in DbContext.GarmentLeftoverWarehouseExpenditureAvalItems on a.Id equals b.AvalExpenditureId
                                      where a._IsDeleted == false && b._IsDeleted == false
                                      && a.ExpenditureDate >= DateFrom
                                      && a.ExpenditureDate <= DateTo
                                      && a.AvalType == "AVAL FABRIC"
                                      select new GarmentLeftoverWarehouseMutationReportViewModel
                                      {
                                          ClassificationCode =  "AV001" ,
                                          ClassificationName = "Aval Besar",
                                          SaldoAwal = 0,
                                          Pemasukan = 0,
                                          Pengeluaran = b.Quantity,
                                          Penyesuaian = 0,
                                          Selisih = 0,
                                          SaldoAkhir = 0,
                                          StockOpname = 0,
                                          //UnitQtyName = b.UomUnit
                                          UnitQtyName = "KG"
                                      }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                      {
                                          ClassificationCode = key.ClassificationCode,
                                          ClassificationName = key.ClassificationName,
                                          SaldoAwal = group.Sum(x => x.SaldoAwal),
                                          Pemasukan = group.Sum(x => x.Pemasukan),
                                          Pengeluaran = group.Sum(x => x.Pengeluaran),
                                          Penyesuaian = group.Sum(x => x.Penyesuaian),
                                          Selisih = group.Sum(x => x.Selisih),
                                          SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                          StockOpname = group.Sum(x => x.StockOpname),
                                          UnitQtyName = key.UnitQtyName

                                      });

            var FilteredExpendAvalKomp = (from a in DbContext.GarmentLeftoverWarehouseExpenditureAvals
                                          join b in DbContext.GarmentLeftoverWarehouseExpenditureAvalItems on a.Id equals b.AvalExpenditureId
                                          where a._IsDeleted == false && b._IsDeleted == false
                                          && a.ExpenditureDate >= DateFrom
                                          && a.ExpenditureDate <= DateTo
                                          && a.AvalType == "AVAL KOMPONEN"
                                          select new GarmentLeftoverWarehouseMutationReportViewModel
                                          {
                                              ClassificationCode = "AV002",
                                              ClassificationName = "Aval Komponen",
                                              SaldoAwal = 0,
                                              Pemasukan = 0,
                                              Pengeluaran = b.Quantity,
                                              Penyesuaian = 0,
                                              Selisih = 0,
                                              SaldoAkhir = 0,
                                              StockOpname = 0,
                                              UnitQtyName = b.UomUnit
                                          }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                          {
                                              ClassificationCode = key.ClassificationCode,
                                              ClassificationName = key.ClassificationName,
                                              SaldoAwal = group.Sum(x => x.SaldoAwal),
                                              Pemasukan = group.Sum(x => x.Pemasukan),
                                              Pengeluaran = group.Sum(x => x.Pengeluaran),
                                              Penyesuaian = group.Sum(x => x.Penyesuaian),
                                              Selisih = group.Sum(x => x.Selisih),
                                              SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                              StockOpname = group.Sum(x => x.StockOpname),
                                              UnitQtyName = key.UnitQtyName

                                          });

            var FilteredExpendAvalPen = (from a in DbContext.GarmentLeftoverWarehouseExpenditureAvals
                                      join b in DbContext.GarmentLeftoverWarehouseExpenditureAvalItems on a.Id equals b.AvalExpenditureId
                                      where a._IsDeleted == false && b._IsDeleted == false
                                      && a.ExpenditureDate >= DateFrom
                                      && a.ExpenditureDate <= DateTo
                                      && a.AvalType == "AVAL BAHAN PENOLONG"
                                         select new GarmentLeftoverWarehouseMutationReportViewModel
                                      {
                                          ClassificationCode = "AV004",
                                          ClassificationName = "Aval Bahan Penolong",
                                          Productname = b.ProductName,
                                          SaldoAwal = 0,
                                          Pemasukan = 0,
                                          Pengeluaran = b.Quantity,
                                          Penyesuaian = 0,
                                          Selisih = 0,
                                          SaldoAkhir = 0,
                                          StockOpname = 0,
                                          UnitQtyName = b.UomUnit
                                      }).GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.Productname, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
                                      {
                                          ClassificationCode = key.ClassificationCode,
                                          ClassificationName = key.ClassificationName,
                                          Productname = key.Productname,
                                          SaldoAwal = group.Sum(x => x.SaldoAwal),
                                          Pemasukan = group.Sum(x => x.Pemasukan),
                                          Pengeluaran = group.Sum(x => x.Pengeluaran),
                                          Penyesuaian = group.Sum(x => x.Penyesuaian),
                                          Selisih = group.Sum(x => x.Selisih),
                                          SaldoAkhir = group.Sum(x => x.SaldoAkhir),
                                          StockOpname = group.Sum(x => x.StockOpname),
                                          UnitQtyName = key.UnitQtyName

                                      });



            //Add Query From Garment -> Aval TC Kecil & Sampah Sapuan -->10/02/2023
            var ReceipTC = (from a in DbContext.GarmentReceiptWasteProductions
                            join b in DbContext.GarmentReceiptWasteProductionItems on a.Id equals b.GarmentReceiptWasteId
                            where a._IsDeleted == false && b._IsDeleted == false
                            && a.ReceiptDate.AddHours(7).Date > BalanceDate.Date
                            && a.ReceiptDate.AddHours(7).Date <= dateTo.Value.Date
                            && a.WasteType == "AVAL TC KECIL"
                            select new GarmentLeftoverWarehouseMutationReportViewModel
                            {
                                ClassificationCode = "ZB05",
                                ClassificationName = "Aval Tc Kecil",
                                Productname = "",
                                SaldoAwal = a.ReceiptDate.AddHours(7).Date < dateFrom ? b.Quantity : 0,
                                Pemasukan = a.ReceiptDate.AddHours(7).Date >= dateFrom ? b.Quantity : 0,
                                Pengeluaran = 0,
                                Penyesuaian = 0,
                                Selisih = 0,
                                SaldoAkhir = 0,
                                StockOpname = 0,
                                UnitQtyName = "KG"
                            });


            var ExpendTC = (from a in DbContext.GarmentExpenditureWasteProductions
                            join b in DbContext.GarmentExpenditureWasteProductionItems on a.Id equals b.GarmentExpenditureWasteId
                            where a._IsDeleted == false && b._IsDeleted == false
                            && a.ExpenditureDate.AddHours(7).Date > BalanceDate.Date
                            && a.ExpenditureDate.AddHours(7).Date <= dateTo.Value.Date
                            && a.WasteType == "AVAL TC KECIL"
                            select new GarmentLeftoverWarehouseMutationReportViewModel
                            {
                                ClassificationCode = "ZB05",
                                ClassificationName = "Aval Tc Kecil",
                                Productname = "",
                                SaldoAwal = a.ExpenditureDate.AddHours(7).Date < dateFrom ? -b.Quantity : 0,
                                Pemasukan =  0,
                                Pengeluaran = a.ExpenditureDate.AddHours(7).Date >= dateFrom ? b.Quantity : 0,
                                Penyesuaian = 0,
                                Selisih = 0,
                                SaldoAkhir = 0,
                                StockOpname = 0,
                                UnitQtyName = "KG"
                            });


            var ReceipSamSap = (from a in DbContext.GarmentReceiptWasteProductions
                            join b in DbContext.GarmentReceiptWasteProductionItems on a.Id equals b.GarmentReceiptWasteId
                            where a._IsDeleted == false && b._IsDeleted == false
                            && a.ReceiptDate.AddHours(7).Date > BalanceDate.Date
                            && a.ReceiptDate.AddHours(7).Date <= dateTo.Value.Date
                            && a.WasteType == "SAMPAH SAPUAN"
                            select new GarmentLeftoverWarehouseMutationReportViewModel
                            {
                                ClassificationCode = "ZA59",
                                ClassificationName = "Sampah Sapuan",
                                Productname = "",
                                SaldoAwal = a.ReceiptDate.AddHours(7).Date < dateFrom ? b.Quantity : 0,
                                Pemasukan = a.ReceiptDate.AddHours(7).Date >= dateFrom ? b.Quantity : 0,
                                Pengeluaran = 0,
                                Penyesuaian = 0,
                                Selisih = 0,
                                SaldoAkhir = 0,
                                StockOpname = 0,
                                UnitQtyName = "KG"
                            });


            var ExpendSamSap = (from a in DbContext.GarmentExpenditureWasteProductions
                            join b in DbContext.GarmentExpenditureWasteProductionItems on a.Id equals b.GarmentExpenditureWasteId
                            where a._IsDeleted == false && b._IsDeleted == false
                            && a.ExpenditureDate.AddHours(7).Date > BalanceDate.Date
                            && a.ExpenditureDate.AddHours(7).Date <= dateTo.Value.Date
                            && a.WasteType == "SAMPAH SAPUAN"
                            select new GarmentLeftoverWarehouseMutationReportViewModel
                            {
                                ClassificationCode = "ZA59",
                                ClassificationName = "Sampah Sapuan",
                                Productname = "",
                                SaldoAwal = a.ExpenditureDate.AddHours(7).Date < dateFrom ? -b.Quantity : 0,
                                Pemasukan = 0,
                                Pengeluaran = a.ExpenditureDate.AddHours(7).Date >= dateFrom ? b.Quantity : 0,
                                Penyesuaian = 0,
                                Selisih = 0,
                                SaldoAkhir = 0,
                                StockOpname = 0,
                                UnitQtyName = "KG"
                            });


            var SAkhir = SaldoAwal.Concat(FilteredReceiptAval).Concat(FilteredReceiptAvalPen).Concat(FilteredReceiptBarangJadi)
                .Concat(FilteredExpendAval)
                .Concat(FilteredExpendAvalPen)
                .Concat(FilteredExpendBarangJadi)
                .Concat(FilteredReceiptAvalKomponen)
                .Concat(FilteredExpendAvalKomp)
                .Concat(ReceipTC)
                .Concat(ExpendTC)
                .Concat(ReceipSamSap)
                .Concat(ExpendSamSap).AsEnumerable();
            //var SAkhir = SaldoAwal.Concat(FilteredReceiptAval).Concat(FilteredReceiptAvalPen).Concat(FilteredReceiptBarangJadi).Concat(FilteredExpendAval).Concat(FilteredExpendAvalPen).Concat(FilteredExpendBarangJadi).AsEnumerable();
            var SaldoAkhir = SAkhir.GroupBy(x => new { x.ClassificationCode, x.ClassificationName, x.Productname, x.UnitQtyName }, (key, group) => new GarmentLeftoverWarehouseMutationReportViewModel
            {
                ClassificationCode = key.ClassificationCode,
                ClassificationName = key.ClassificationName,
                Productname = key.Productname,
                SaldoAwal = Math.Round(group.Sum(x => x.SaldoAwal),2),
                Pemasukan = Math.Round(group.Sum(x => x.Pemasukan),2),
                Pengeluaran = Math.Round(group.Sum(x => x.Pengeluaran),2),
                Penyesuaian = Math.Round(group.Sum(x => x.Penyesuaian), 2),
                Selisih = Math.Round(group.Sum(x => x.Selisih), 2),
                SaldoAkhir = Math.Round(group.Sum(x => x.SaldoAwal) + group.Sum(x => x.Pemasukan) - group.Sum(x => x.Pengeluaran), 2),
                StockOpname = Math.Round(group.Sum(x => x.StockOpname), 2),
                UnitQtyName = key.UnitQtyName

            }).ToList();

            //var mutationScrap = GetScrap(DateFrom.Date, DateTo.Date);

            //foreach (var mm in mutationScrap)
            //{
            //    //var saldoawal = mm.SaldoAwal < 0 ? 0 : mm.SaldoAwal;
            //    //var saldoakhir = mm.SaldoAkhir < 0 ? 0 : mm.SaldoAkhir;

            //    //mm.SaldoAwal = saldoawal;
            //    //mm.SaldoAkhir = saldoakhir;
            //    mm.ClassificationName = (mm.ClassificationCode == "ZB05" ? "Aval Tc Kecil" :  "Sampah Sapuan" );
            //    //mm.ClassificationName = (mm.ClassificationCode == "ZB05" ? "Aval Tc Kecil" : mm.ClassificationCode == "ZA59" ? "Sampah Sapuan" : "Aval Komponen");
            //}

            //if (mutationScrap.Count == 0)
            //{
            //    mutationScrap.Add(new GarmentLeftoverWarehouseMutationReportViewModel
            //    {
            //        ClassificationCode = "ZB05",
            //        ClassificationName = "Aval Tc Kecil",
            //        SaldoAwal = 0,
            //        Pemasukan = 0,
            //        Pengeluaran = 0,
            //        Penyesuaian = 0,
            //        Selisih = 0,
            //        SaldoAkhir = 0,
            //        StockOpname = 0,
            //        UnitQtyName = "KG"
            //    });

            //    mutationScrap.Add(new GarmentLeftoverWarehouseMutationReportViewModel
            //    {
            //        ClassificationCode = "ZA59",
            //        ClassificationName = "Sampah Sapuan",
            //        SaldoAwal = 0,
            //        Pemasukan = 0,
            //        Pengeluaran = 0,
            //        Penyesuaian = 0,
            //        Selisih = 0,
            //        SaldoAkhir = 0,
            //        StockOpname = 0,
            //        UnitQtyName = "KG"
            //    });
            //}

            if (SaldoAkhir.FirstOrDefault(x => x.ClassificationName == "Aval Komponen") == null)
            {
                SaldoAkhir.Add(new GarmentLeftoverWarehouseMutationReportViewModel
                {
                    ClassificationCode = "AV002",
                    ClassificationName = "Aval Komponen",
                    SaldoAwal = 0,
                    Pemasukan = 0,
                    Pengeluaran = 0,
                    Penyesuaian = 0,
                    Selisih = 0,
                    SaldoAkhir = 0,
                    StockOpname = 0,
                    UnitQtyName = "KG"
                });
            }
            if (SaldoAkhir.FirstOrDefault(x => x.ClassificationName == "Aval Bahan Penolong") == null)
            {
                SaldoAkhir.Add(new GarmentLeftoverWarehouseMutationReportViewModel
                {
                    ClassificationCode = "AV004",
                    ClassificationName = "Aval Bahan Penolong",
                    Productname = "",
                    SaldoAwal = 0,
                    Pemasukan = 0,
                    Pengeluaran = 0,
                    Penyesuaian = 0,
                    Selisih = 0,
                    SaldoAkhir = 0,
                    StockOpname = 0,
                    UnitQtyName = "KG"
                });
            }
            if (SaldoAkhir.FirstOrDefault(x => x.ClassificationName == "Aval Besar") == null)
            {
                SaldoAkhir.Add(new GarmentLeftoverWarehouseMutationReportViewModel
                {
                    ClassificationCode = "AV001",
                    ClassificationName = "Aval Besar",
                    SaldoAwal = 0,
                    Pemasukan = 0,
                    Pengeluaran = 0,
                    Penyesuaian = 0,
                    Selisih = 0,
                    SaldoAkhir = 0,
                    StockOpname = 0,
                    UnitQtyName = "KG"
                });
            }
            if (SaldoAkhir.FirstOrDefault(x => x.ClassificationName == "Barang Jadi Reject") == null)
            {
                SaldoAkhir.Add(new GarmentLeftoverWarehouseMutationReportViewModel
                {
                    ClassificationCode = "RJ001",
                    ClassificationName = "Barang Jadi Reject",
                    SaldoAwal = 0,
                    Pemasukan = 0,
                    Pengeluaran = 0,
                    Penyesuaian = 0,
                    Selisih = 0,
                    SaldoAkhir = 0,
                    StockOpname = 0,
                    UnitQtyName = "PCS"
                });
            };

            if (SaldoAkhir.FirstOrDefault(x => x.ClassificationName == "Aval Tc Kecil") == null)
            {
                SaldoAkhir.Add(new GarmentLeftoverWarehouseMutationReportViewModel
                {
                    ClassificationCode = "ZB05",
                    ClassificationName = "Aval Tc Kecil",
                    SaldoAwal = 0,
                    Pemasukan = 0,
                    Pengeluaran = 0,
                    Penyesuaian = 0,
                    Selisih = 0,
                    SaldoAkhir = 0,
                    StockOpname = 0,
                    UnitQtyName = "KG"
                });
            };

            if (SaldoAkhir.FirstOrDefault(x => x.ClassificationName == "Sampah Sapuan") == null)
            {
                SaldoAkhir.Add(new GarmentLeftoverWarehouseMutationReportViewModel
                {
                    ClassificationCode = "ZA59",
                    ClassificationName = "Sampah Sapuan",
                    SaldoAwal = 0,
                    Pemasukan = 0,
                    Pengeluaran = 0,
                    Penyesuaian = 0,
                    Selisih = 0,
                    SaldoAkhir = 0,
                    StockOpname = 0,
                    UnitQtyName = "KG"
                });
            };



            //var mutation = SaldoAkhir.Concat(mutationScrap).ToList();

            return SaldoAkhir.OrderBy(x => x.ClassificationCode).ToList();

        }


        private List<GarmentLeftoverWarehouseMutationReportViewModel> GetScrap(DateTime datefrom, DateTime dateTo)
        {
            IHttpService httpClient = (IHttpService)this.ServiceProvider.GetService(typeof(IHttpService));

            var garmentProductionUri = APIEndpoint.GarmentProduction + $"scrap-transactions/mutation";
            string queryUri = "?dateFrom=" + datefrom + "&dateTo=" + dateTo;
            string uri = garmentProductionUri + queryUri;
            var httpResponse = httpClient.GetAsync($"{uri}").Result;

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> Result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                List<GarmentLeftoverWarehouseMutationReportViewModel> viewModel;
                if (Result.GetValueOrDefault("data") == null)
                {
                    viewModel = null;
                }
                else
                {
                    viewModel = JsonConvert.DeserializeObject<List<GarmentLeftoverWarehouseMutationReportViewModel>>(Result.GetValueOrDefault("data").ToString());

                }
                return viewModel;
            }
            else
            {
                List<GarmentLeftoverWarehouseMutationReportViewModel> viewModel = new List<GarmentLeftoverWarehouseMutationReportViewModel>();
                return viewModel;
            }
        }

        public MemoryStream GenerateExcelMutation(DateTime dateFrom, DateTime dateTo)
        {
            var Query = GetReportQuery(dateFrom, dateTo);
            DataTable Result = new DataTable();
            Result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Kategori Barang", DataType = typeof(String) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(string) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Barang", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Saldo Awal", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Pemasukan Barang", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Pengeluaran Barang", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Penyesuaian (Adjustment)", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Saldo Akhir", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Hasil Pencacahan", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Selisih", DataType = typeof(double) });
            Result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            ExcelPackage package = new ExcelPackage();
            if (Query.ToArray().Count() == 0)
                Result.Rows.Add("", "", "", "", "", 0, 0, 0, 0, 0, 0, 0, 0, "");
            else
            {
                int counter = 5;
                int idx = 1;
                var rCount = 0;
                int indexNum = 1;
                Dictionary<string, string> Rowcount = new Dictionary<string, string>();

                foreach (var item in Query)
                {
                    if (item.ClassificationCode == "AV004")
                    {
                        idx++;
                        if (!Rowcount.ContainsKey(item.ClassificationCode))
                        {
                            rCount = 0;
                            var index = idx;
                            Rowcount.Add(item.ClassificationCode, index.ToString());
                        }
                        else
                        {
                            rCount += 1;
                            Rowcount[item.ClassificationCode] = Rowcount[item.ClassificationCode] + "-" + rCount.ToString();
                            var val = Rowcount[item.ClassificationCode].Split("-");
                            if ((val).Length > 0)
                            {
                                Rowcount[item.ClassificationCode] = val[0] + "-" + rCount.ToString();
                            }
                        }
                    }
                    Result.Rows.Add(indexNum.ToString(),item.ClassificationCode, item.ClassificationName, item.Productname, item.UnitQtyName,0, item.SaldoAwal, item.Pemasukan, item.Pengeluaran, item.Penyesuaian, item.SaldoAkhir, item.StockOpname, item.Selisih,"-");
                    counter++;
                    indexNum++;
                }
                bool styling = true;

                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(Result, "ScrapReject") })
                {
                    var sheet = package.Workbook.Worksheets.Add(item.Value);
                    sheet.Cells[$"A1:N1"].Style.Font.Bold = true;
                    sheet.Cells["A1"].LoadFromDataTable(item.Key, true, (styling == true) ? OfficeOpenXml.Table.TableStyles.Light16 : OfficeOpenXml.Table.TableStyles.Light16);
                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    foreach (var rowMerge in Rowcount)
                    {

                        var UnitrowNum = rowMerge.Value.Split("-");
                        int rowNum2 = 1;
                        int rowNum1 = Convert.ToInt32(UnitrowNum[0]);
                        if (UnitrowNum.Length > 1)
                        {
                            rowNum2 = Convert.ToInt32(rowNum1) + Convert.ToInt32(UnitrowNum[1]);
                        }
                        else
                        {
                            rowNum2 = Convert.ToInt32(rowNum1);
                        }

                        sheet.Cells[$"A{(rowNum1 + 3)}:A{(rowNum2) + 3}"].Merge = true;
                        sheet.Cells[$"A{(rowNum1 + 3)}:A{(rowNum2) + 3}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        sheet.Cells[$"A{(rowNum1 + 3)}:A{(rowNum2) + 3}"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        sheet.Cells[$"B{(rowNum1 + 3)}:B{(rowNum2 + 3)}"].Merge = true;
                        sheet.Cells[$"B{(rowNum1 + 3)}:B{(rowNum2 + 3)}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        sheet.Cells[$"B{(rowNum1 + 3)}:B{(rowNum2 + 3)}"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    }
                }
            }
            var stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
            //return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(Result, "ScrapReject") }, true);
        }
    }
}

