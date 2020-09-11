﻿using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIMongoDataService.DataService
{
	public class DatafeedDataService : IDatafeedDataService
	{
		static string databaseName = "finance";
		static string datafeedTableName = "datafeeds";
		static string externalAccountMappingsTable = "datafeed_account_mappings";
		string connectionString;

		public DatafeedDataService(string connectionString)
		{
			this.connectionString = connectionString;
		}

		public bool AddAccountDatafeedMapping(string clientId, string datafeed, string vendorID, string accountID, string externalAccountID)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var record = new
			{
				_id = $"{accountID}_{externalAccountID}",
				accountId = accountID,
				externalId = externalAccountID,
				datafeed,
				vendorID,
				clientId
			};
			return database.InsertRecord(externalAccountMappingsTable, record);
		}

		public bool RemoveAccountDatafeedMapping(string clientId, string accountID, string externalAccountID)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<dynamic>.Filter.Eq("clientId", clientId);

			return database.DeleteRecord(externalAccountMappingsTable, $"{accountID}_{externalAccountID}", filter, "_id");
		}

		public bool AddUpdateClientDatafeed(Datafeed datafeed)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.UpsertRecord(datafeedTableName, datafeed, datafeed._id);
		}

		public bool DeleteClientDatafeed(string clientId, string provider, string vendorID)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var clientFilter = Builders<Datafeed>.Filter.Eq("ClientId", clientId);
			var filter = Builders<Datafeed>.Filter.Eq("datafeed", provider) & Builders<Datafeed>.Filter.Eq("vendorID", vendorID);

			database.DeleteManyRecords(externalAccountMappingsTable, filter);

			return database.DeleteRecord(datafeedTableName, new Datafeed(clientId, provider, vendorID, null, DateTime.MinValue)._id, clientFilter, "_id");
		}

		public List<Datafeed> GetDatafeeds(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Datafeed>.Filter.Eq("ClientId", clientId);
			return database.LoadRecordsByFilter(datafeedTableName, filter);
		}

		public List<ExternalAccount> GetExternalAccounts(string clientId, string accountId = null)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<dynamic>.Filter.Eq("clientId", clientId);
			if(!string.IsNullOrEmpty(accountId))
				filter &= Builders<dynamic>.Filter.Eq("accountId", accountId);

			List<dynamic> result = database.LoadRecordsByFilter(externalAccountMappingsTable, filter);
			List<ExternalAccount> externalAccounts = new List<ExternalAccount>();

			result.ForEach(r => externalAccounts.Add(new ExternalAccount(r.externalId, r.externalId, r.vendorID, "", r.datafeed, true, r.accountId)));
			return externalAccounts;
		}

		public string GetRefreshTokenByAccessKey(string encryptedAccesskey)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Datafeed>.Filter.Eq("AccessKey", encryptedAccesskey);
			var datafeeds = database.LoadRecordsByFilter(datafeedTableName, filter);
			if (datafeeds != null && datafeeds.Count > 0)
				return datafeeds[0].RefreshKey;

			return null;
		}

		public bool IsExternalAccountMapped(string clientId, string externalAccountID, string vendorID, out string mappedAccount)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var accountFilter = Builders<dynamic>.Filter.Eq("externalId", externalAccountID);
			var vendorFilter = Builders<dynamic>.Filter.Eq("vendorID", vendorID);
			var clientFilter = Builders<dynamic>.Filter.Eq("clientId", clientId);
			
			var accounts = database.LoadRecordsByFilter(externalAccountMappingsTable, accountFilter & vendorFilter & clientFilter);
			if(accounts != null && accounts.Count > 0)
			{
				mappedAccount = accounts[0].accountId;
				return true;
			}

			mappedAccount = null;
			return false;
		}

		public bool UpdateAccessKey(string newAccessKey, string newRefreshToken, string oldAccessKey, DateTime lastUpdated)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var update = Builders<Datafeed>.Update.Set(d => d.AccessKey, newAccessKey);
			update = update.Set(d => d.RefreshKey, newRefreshToken);

			var filter = Builders<Datafeed>.Filter.Eq(d => d.AccessKey, oldAccessKey);

			return database.PartialUpdateRecord(datafeedTableName, update, filter);
		}

		public string GetAccessKeyForExternalAccount(string provider, string vendorId,  string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Datafeed>.Filter.Eq(d => d.Provider, provider) &
						Builders<Datafeed>.Filter.Eq(d => d.VendorID, vendorId) &
						Builders<Datafeed>.Filter.Eq(d => d.ClientId, clientId);
			var feeds = database.LoadRecordsByFilter(datafeedTableName, filter);
			if (feeds.Count == 1)
				return feeds[0].AccessKey;

			return null;
		}

		public bool RemoveAllAccountDatafeedMappings(string clientId, string accountID)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<dynamic>.Filter.Eq("clientId", clientId) & Builders<dynamic>.Filter.Eq("accountId", accountID);

			return database.DeleteManyRecords(externalAccountMappingsTable, filter);
		}
	}
}
