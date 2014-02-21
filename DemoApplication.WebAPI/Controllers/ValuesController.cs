using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DemoApplication.WebAPI.Models;

namespace DemoApplication.WebAPI.Controllers {
	public class ValuesController : ApiController {
		private static Random _random = new Random((int)DateTime.Now.Ticks);

		/// <summary>
		/// Hits an endpoint to warm up the server.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public string Startup() {
			return "Started";
		}

		// http://localhost:8085/api/values/get/5
		/// <summary>
		/// Gets an automatically generated "LOS Lite" response payload.
		/// </summary>
		/// <param name="count">The number of results to return.</param>
		/// <param name="childPercent">The percentage of the time that a new child node should be created.  Default is 50.</param>
		/// <param name="siblingPercent">The percentage of the time that a new sibling node should be created vs. returning to a parent.  Default is 50.</param>
		/// <returns></returns>
		public ABOResult Get(int count, int childPercent = 50, int siblingPercent = 50) {
			var results = this.Getter(count, false, childPercent, siblingPercent);
			return results;
		}

		/// <summary>
		/// Gets an automatically generated "Details" response payload.
		/// </summary>
		/// <param name="count">The number of results to return.</param>
		/// <param name="childPercent">The percentage of the time that a new child node should be created.  Default is 50.</param>
		/// <param name="siblingPercent">The percentage of the time that a new sibling node should be created vs. returning to a parent.  Default is 50.</param>
		/// <returns></returns>
		[HttpGet]
		public ABOResult GetFull(int count, int childPercent = 50, int siblingPercent = 50) {
			var results = this.Getter(count, true, childPercent, siblingPercent);
			return results;
		}

		//http://localhost:8085/api/values/getminified/5
		/// <summary>
		/// Gets an automatically generated "LOS Lite" response payload that is minified by removing the key names of the
		/// JSON properties.
		/// </summary>
		/// <param name="count">The number of results to return.</param>
		/// <param name="childPercent">The percentage of the time that a new child node should be created.  Default is 50.</param>
		/// <param name="siblingPercent">The percentage of the time that a new sibling node should be created vs. returning to a parent.  Default is 50.</param>
		/// <returns></returns>
		[HttpGet]
		public List<object> GetMinified(int count, int childPercent = 50, int siblingPercent = 50) {
			var parent = this.Get(count, childPercent, siblingPercent);
			var result = this.Minify(parent);
			return result;
		}

		#region Methods
		private ABOResult Getter(int count, bool getFull, int childPercent, int siblingPercent) {
			// limit the number of results
			if (count > 20000) count = 20000;
			count = count - 1;

			var results = new List<ABOResult>();
			NamesList names = new NamesList();

			ABOResult result = (getFull) ? new ABOFullResult() : new ABOResult();
			result.SetSampleData(names, 0);
			int index = 0;
			while (index < count) {
				this.Populate(result, count, ref index, getFull, names, childPercent, siblingPercent);
			}

			return result;
		}

		private void Populate(ABOResult parent, int count, ref int index, bool getFull, NamesList names, int childPercent, int siblingPercent) {
			if (index >= count) return;

			index++;
			
			ABOResult result = (getFull) ? new ABOFullResult() : new ABOResult();
			result.SetSampleData(names, index);
			if (parent.Children == null) parent.Children = new List<ABOResult>();
			parent.Children.Add(result);

			result.GroupSize = _random.Next(1, 25);

			bool makeChild = (((double)_random.Next(10000) / 10000) * 100) <= childPercent;
			bool makeSibling = (((double)_random.Next(10000) / 10000) * 100) <= siblingPercent;
			if (makeChild) {
				this.Populate(result, count, ref index, getFull, names, childPercent, siblingPercent);
			} else if (makeSibling) {
				this.Populate(parent, count, ref index, getFull, names, childPercent, siblingPercent);
			}
		}

		private List<object> Minify(ABOResult record) {
			var resultStrings = new List<object>();

			var properties = record.GetType().GetProperties();
			foreach (var item in properties) {
				if (item.Name != "Children") {
					resultStrings.Add(item.GetValue(record));
				}
			}

			//var resultStrings = new List<object>(new object[] {
			//		record.Name,
			//		record.ID,
			//		record.Country,
			//		record.IsActive.ToString(),
			//		record.IsConfidential.ToString(),
			//		record.Entry.ToString()
			//});

			if (record.Children != null) {
				List<object> children = new List<object>();
				foreach (var child in record.Children) {
					children.Add(this.Minify(child));
				}
				resultStrings.Add(children);
			}

			return resultStrings;
		}

		public static bool GetPercentage(int percentageTrue) {
			double randVal = ((double)_random.Next(100000) / 100000) * 100;

			return (randVal <= percentageTrue);
		}
		#endregion

		#region SubClasses
		public class ABOResult {
			public string Name { get; set; }
			public string ID { get; set; }
			public int GroupSize { get; set; }
			public string Aff { get; set; }
			public DateTime Entry { get; set; }
			public bool IsActive { get; set; }
			public bool IsConfidential { get; set; }
			public bool IsInternational { get; set; }
			public List<ABOResult> Children { get; set; }

			public virtual void SetSampleData(NamesList names, int count) {
				this.Name = names.GetName();
				this.ID = "10000" + count.ToString();
				this.Aff = "US";
				this.IsActive = GetPercentage(90);
				this.IsConfidential = GetPercentage(10);
				this.IsInternational = GetPercentage(10);
				this.Entry = new DateTime(_random.Next(1950, 2010), _random.Next(1, 13), _random.Next(1, 28));
				this.GroupSize = 0;
			}
		}

		public class ABONumber {
			public string Aff { get; set; }
			public string ABONo { get; set; }
		}

		public class Award {
			public string Code { get; set; }
			public string Name { get; set; }
			public int Rank { get; set; }
			public int QualificationPeriod { get; set; }
		}

		public class Extended {
			public string PrimaryName { get; set; }
			public int PrimaryPhoneNo { get; set; }
			public string PrimaryEmail { get; set; }
		}

		public class ABOFullResult : ABOResult {
			public ABONumber FosterSponsorABO { get; set; }
			public ABONumber UplinePlatinumABO { get; set; }
			public Award CurrentAward { get; set; }
			public Award HighestAward { get; set; }
			public Extended Extended { get; set; }
			public VolumeContainer Volume { get; set; }

			public override void SetSampleData(NamesList names, int count) {
				base.SetSampleData(names, count);

				this.Volume = new VolumeContainer() {
					BonusPeriod = 201410,
					Results = new List<VolumeResult>() {
						new VolumeResult() {
							BV = 131.13,
							PV = 140.11
						}
					}
				};
			}
		}

		public class VolumeContainer {
			public long BonusPeriod { get; set; }
			public List<VolumeResult> Results { get; set; }
		}

		public class VolumeResult {
			public double PV { get; set; }
			public double BV { get; set; }
		}
		#endregion

	}
}