using System;
using System.Collections.Generic;
using NUnit.Framework;
using Service.Circle.Wallets.Domain.Models;
using Service.ClientRiskManager.Domain;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Tests
{
    public class TestDepositLimitPerDay
    {
        private CircleCardPaymentDetails _paymentDetails = new CircleCardPaymentDetails();
        private readonly DateTime _currDate = DateTime.UtcNow;

        [SetUp]
        public void Setup()
        {
            _paymentDetails.Day1Limit = 1000m;
            _paymentDetails.Day7Limit = 3000m;
            _paymentDetails.Day30Limit = 12000m;
        }

        [Test]
        public void Day1_Amount_Less_Limit()
        {
            var depo1 = new CircleClientDeposit
            {
                Date = _currDate.AddHours(-1),
                Balance = 1m,
                BalanceInUsd = 500m,
                AssetSymbol = "BTC"
            };
            var cardDeposits = new List<CircleClientDeposit>();
            cardDeposits.Add(depo1);
            
            var cardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                _paymentDetails, cardDeposits, _currDate);
            
            Assert.IsTrue(cardDepositsSummary.Deposit1DaysState == LimitState.Active);
            Assert.IsTrue(cardDepositsSummary.Deposit7DaysState == LimitState.None);
            Assert.IsTrue(cardDepositsSummary.Deposit30DaysState == LimitState.None);
            Assert.IsTrue(cardDepositsSummary.LastDeposit1DaysLeftHours == 23);
            Assert.IsTrue(cardDepositsSummary.LastDeposit7DaysLeftHours == 167);
            Assert.IsTrue(cardDepositsSummary.LastDeposit30DaysLeftHours == 719);
            Assert.IsTrue(cardDepositsSummary.BarInterval == BarState.Day1);
            Assert.IsTrue(cardDepositsSummary.BarProgres == 50);
            Assert.IsTrue(cardDepositsSummary.LeftHours == 0);
        }
        
        [Test]
        public void Day1_Amount_Equal_Limit()
        {
            var depo1 = new CircleClientDeposit
            {
                Date = _currDate,
                Balance = 1m,
                BalanceInUsd = 1000m,
                AssetSymbol = "BTC"
            };
            
            var cardDeposits = new List<CircleClientDeposit>();
            cardDeposits.Add(depo1);
            
            var cardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                _paymentDetails, cardDeposits, _currDate);

            Assert.IsTrue(cardDepositsSummary.Deposit1DaysState == LimitState.Block);
            Assert.IsTrue(cardDepositsSummary.Deposit7DaysState == LimitState.Active);
            Assert.IsTrue(cardDepositsSummary.Deposit30DaysState == LimitState.None);
            Assert.IsTrue(cardDepositsSummary.LastDeposit1DaysLeftHours == 24);
            Assert.IsTrue(cardDepositsSummary.LastDeposit7DaysLeftHours == 168);
            Assert.IsTrue(cardDepositsSummary.LastDeposit30DaysLeftHours == 720);
            Assert.IsTrue(cardDepositsSummary.BarInterval == BarState.Day7);
            Assert.IsTrue(cardDepositsSummary.BarProgres == 33);
            Assert.IsTrue(cardDepositsSummary.LeftHours == 24);
        }
        
        [Test]
        public void Day1_Amount_Full_Day2_Less_Limit()
        {
            var depo1 = new CircleClientDeposit
            {
                Date = _currDate,
                Balance = 1m,
                BalanceInUsd = 1000m,
                AssetSymbol = "BTC"
            };
            
            var depo2 = new CircleClientDeposit
            {
                Date = _currDate.AddHours(-1),
                Balance = 1m,
                BalanceInUsd = 1000m,
                AssetSymbol = "BTC"
            };
            var cardDeposits = new List<CircleClientDeposit>();
            cardDeposits.Add(depo1);
            cardDeposits.Add(depo2);
            
            var cardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                _paymentDetails, cardDeposits, _currDate);

            Assert.IsTrue(cardDepositsSummary.Deposit1DaysState == LimitState.Block);
            Assert.IsTrue(cardDepositsSummary.Deposit7DaysState == LimitState.Active);
            Assert.IsTrue(cardDepositsSummary.Deposit30DaysState == LimitState.None);
            Assert.IsTrue(cardDepositsSummary.LastDeposit1DaysLeftHours == 23);
            Assert.IsTrue(cardDepositsSummary.LastDeposit7DaysLeftHours == 167);
            Assert.IsTrue(cardDepositsSummary.LastDeposit30DaysLeftHours == 719);
            Assert.IsTrue(cardDepositsSummary.BarInterval == BarState.Day7);
            Assert.IsTrue(cardDepositsSummary.BarProgres == 67);
            Assert.IsTrue(cardDepositsSummary.LeftHours == 23);
        }
        
        [Test]
        public void Day1_Day2_Day3_Full()
        {
            var depo1 = new CircleClientDeposit
            {
                Date = _currDate.AddMinutes(-5),
                Balance = 1m,
                BalanceInUsd = 1000m,
                AssetSymbol = "BTC"
            };
            
            var depo2 = new CircleClientDeposit
            {
                Date = _currDate.AddHours(-23),
                Balance = 1m,
                BalanceInUsd = 2000m,
                AssetSymbol = "BTC"
            };
            
            var depo3 = new CircleClientDeposit
            {
                Date = _currDate.AddDays(-8),
                Balance = 1m,
                BalanceInUsd = 9000m,
                AssetSymbol = "BTC"
            };
            var cardDeposits = new List<CircleClientDeposit>();
            cardDeposits.Add(depo1);
            cardDeposits.Add(depo2);
            cardDeposits.Add(depo3);

            var cardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                _paymentDetails, cardDeposits, _currDate);

            Assert.IsTrue(cardDepositsSummary.Deposit1DaysState == LimitState.Block);
            Assert.IsTrue(cardDepositsSummary.Deposit7DaysState == LimitState.Block);
            Assert.IsTrue(cardDepositsSummary.Deposit30DaysState == LimitState.Block);
            Assert.IsTrue(cardDepositsSummary.LastDeposit1DaysLeftHours == 1);
            Assert.IsTrue(cardDepositsSummary.LastDeposit7DaysLeftHours == 145);
            Assert.IsTrue(cardDepositsSummary.LastDeposit30DaysLeftHours == 528);
            Assert.IsTrue(cardDepositsSummary.BarInterval == BarState.Day30);
            Assert.IsTrue(cardDepositsSummary.BarProgres == 100);
            Assert.IsTrue(cardDepositsSummary.LeftHours == 528);
        }
    }
}
