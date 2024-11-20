#nullable enable
using Serilog;
using SteamWebAuthenticator.Helpers;

namespace SteamWebAuthenticator.Models;

public abstract class SerializableFile : IDisposable {
	
	private static readonly SemaphoreSlim GlobalFileSemaphore = new(1, 1);

	private readonly SemaphoreSlim _fileSemaphore = new(1, 1);

	public string? Username { get; set; }
	
	public string? FilePath => $"{Username}{Constants.JsonExtension}";

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing) {
		if (disposing) {
			_fileSemaphore.Dispose();
		}
	}

	public abstract Task SaveAsync();

	protected static async Task SaveAsync<T>(T serializableFile) where T : SerializableFile {
		ArgumentNullException.ThrowIfNull(serializableFile);

		if (string.IsNullOrEmpty(serializableFile.FilePath)) {
			throw new InvalidOperationException(nameof(serializableFile.FilePath));
		}

		await serializableFile._fileSemaphore.WaitAsync().ConfigureAwait(false);

		try
		{
			string json = serializableFile.ToJsonText();

			if (string.IsNullOrEmpty(json))
			{
				throw new InvalidOperationException("Serialized JSON content is null or empty.");
			}

			// Combine file paths upfront
			string filePath = Path.Combine(Constants.Accounts, serializableFile.FilePath);
			string newFilePath = $"{filePath}.new";

			if (File.Exists(filePath))
			{
				string currentJson = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

				if (json == currentJson)
				{
					return;
				}
			}

			await File.WriteAllTextAsync(newFilePath, json).ConfigureAwait(false);

			if (File.Exists(filePath))
			{
				File.Replace(newFilePath, filePath, null);
			}
			else
			{
				File.Move(newFilePath, filePath);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to write the file safely.");
		}
		finally
		{
			serializableFile._fileSemaphore.Release();

			string tempFilePath = $"{Path.Combine(Constants.Accounts, serializableFile.FilePath)}.new";
			if (File.Exists(tempFilePath))
			{
				try
				{
					File.Delete(tempFilePath);
				}
				catch (Exception cleanupEx)
				{
					Log.Warning(cleanupEx, "Failed to clean up temporary file.");
				}
			}
		}

	}

	internal static async Task<bool> Write(string filePath, string json) {
		ArgumentException.ThrowIfNullOrEmpty(filePath);
		ArgumentException.ThrowIfNullOrEmpty(json);

		string newFilePath = $"{filePath}.new";

		await GlobalFileSemaphore.WaitAsync().ConfigureAwait(false);

		try {
			// We always want to write entire content to temporary file first, in order to never load corrupted data, also when target file doesn't exist
			if (File.Exists(filePath)) {
				string currentJson = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

				if (json == currentJson) {
					return true;
				}

				await File.WriteAllTextAsync(newFilePath, json).ConfigureAwait(false);

				File.Replace(newFilePath, filePath, null);
			} else {
				await File.WriteAllTextAsync(newFilePath, json).ConfigureAwait(false);

				File.Move(newFilePath, filePath);
			}

			return true;
		} catch (Exception e) {
			Log.Error(e.Message + e.StackTrace);
			return false;
		} finally {
			GlobalFileSemaphore.Release();
		}
	}
}