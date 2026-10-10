namespace EcoGuide.Data.Exceptions;

/// <summary>
/// Exception to throw when a document is not found in the database.
/// </summary>
public class DocumentNotFoundException<TDocument>(string documentId) : DocumentNotFoundException($"{typeof(TDocument).Name} with id {documentId} does not exists.")
{ }

/// <summary>
/// Base exception.
/// </summary>
public abstract class DocumentNotFoundException(string message) : Exception(message)
{ }