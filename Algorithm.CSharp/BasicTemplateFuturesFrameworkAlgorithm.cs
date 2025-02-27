/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template futures framework algorithm uses framework components to define an algorithm
    /// that trades futures.
    /// </summary>
    public class BasicTemplateFuturesFrameworkAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        protected virtual bool ExtendedMarketHours => false;

        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Minute;
            UniverseSettings.ExtendedMarketHours = ExtendedMarketHours;

            SetStartDate(2013, 10, 07);
            SetEndDate(2013, 10, 11);
            SetCash(100000);

            // set framework models
            SetUniverseSelection(new FrontMonthFutureUniverseSelectionModel(SelectFutureChainSymbols));
            SetAlpha(new ConstantFutureContractAlphaModel(InsightType.Price, InsightDirection.Up, TimeSpan.FromDays(1)));
            SetPortfolioConstruction(new SingleSharePortfolioConstructionModel());
            SetExecution(new ImmediateExecutionModel());
            SetRiskManagement(new NullRiskManagementModel());
        }

        // future symbol universe selection function
        private static IEnumerable<Symbol> SelectFutureChainSymbols(DateTime utcTime)
        {
            var newYorkTime = utcTime.ConvertFromUtc(TimeZones.NewYork);
            if (newYorkTime.Date < new DateTime(2013, 10, 09))
            {
                yield return QuantConnect.Symbol.Create(Futures.Indices.SP500EMini, SecurityType.Future, Market.CME);
            }

            if (newYorkTime.Date >= new DateTime(2013, 10, 09))
            {
                yield return QuantConnect.Symbol.Create(Futures.Metals.Gold, SecurityType.Future, Market.COMEX);
            }
        }

        /// <summary>
        /// Creates futures chain universes that select the front month contract and runs a user
        /// defined futureChainSymbolSelector every day to enable choosing different futures chains
        /// </summary>
        class FrontMonthFutureUniverseSelectionModel : FutureUniverseSelectionModel
        {
            public FrontMonthFutureUniverseSelectionModel(Func<DateTime, IEnumerable<Symbol>> futureChainSymbolSelector)
                : base(TimeSpan.FromDays(1), futureChainSymbolSelector)
            {
            }

            /// <summary>
            /// Defines the future chain universe filter
            /// </summary>
            protected override FutureFilterUniverse Filter(FutureFilterUniverse filter)
            {
                return filter
                    .FrontMonth()
                    .OnlyApplyFilterAtMarketOpen();
            }
        }

        /// <summary>
        /// Implementation of a constant alpha model that only emits insights for future symbols
        /// </summary>
        class ConstantFutureContractAlphaModel : ConstantAlphaModel
        {
            public ConstantFutureContractAlphaModel(InsightType type, InsightDirection direction, TimeSpan period)
                : base(type, direction, period)
            {
            }

            protected override bool ShouldEmitInsight(DateTime utcTime, Symbol symbol)
            {
                // only emit alpha for future symbols and not underlying equity symbols
                if (symbol.SecurityType != SecurityType.Future)
                {
                    return false;
                }

                return base.ShouldEmitInsight(utcTime, symbol);
            }
        }

        /// <summary>
        /// Portfolio construction model that sets target quantities to 1 for up insights and -1 for down insights
        /// </summary>
        class SingleSharePortfolioConstructionModel : PortfolioConstructionModel
        {
            public override IEnumerable<IPortfolioTarget> CreateTargets(QCAlgorithm algorithm, Insight[] insights)
            {
                foreach (var insight in insights)
                {
                    yield return new PortfolioTarget(insight.Symbol, (int) insight.Direction);
                }
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public virtual bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public virtual Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public virtual long DataPoints => 43786;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public virtual int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public virtual Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "-81.734%"},
            {"Drawdown", "4.100%"},
            {"Expectancy", "0"},
            {"Net Profit", "-2.169%"},
            {"Sharpe Ratio", "-10.195"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-1.206"},
            {"Beta", "0.238"},
            {"Annual Standard Deviation", "0.072"},
            {"Annual Variance", "0.005"},
            {"Information Ratio", "-15.404"},
            {"Tracking Error", "0.176"},
            {"Treynor Ratio", "-3.077"},
            {"Total Fees", "$4.62"},
            {"Estimated Strategy Capacity", "$17000000.00"},
            {"Lowest Capacity Asset", "GC VL5E74HP3EE5"},
            {"Fitness Score", "0.006"},
            {"Kelly Criterion Estimate", "-50.022"},
            {"Kelly Criterion Probability Value", "0.711"},
            {"Sortino Ratio", "-10.107"},
            {"Return Over Maximum Drawdown", "-51.815"},
            {"Portfolio Turnover", "0.54"},
            {"Total Insights Generated", "5"},
            {"Total Insights Closed", "4"},
            {"Total Insights Analysis Completed", "4"},
            {"Long Insight Count", "5"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$-4434.791"},
            {"Total Accumulated Estimated Alpha Value", "$-720.6535"},
            {"Mean Population Estimated Insight Value", "$-180.1634"},
            {"Mean Population Direction", "25%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "25%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "323b899ae80aa839e320806411665ce7"}
        };
    }
}
