USE coordinateqa
GO

DECLARE @objectName VARCHAR(150) = 'client'

SELECT ao.[name] ObjectName
     , ao.[object_id] ObjectId
	 , ao.[type_desc] ObjectType
	 , s.[name] SchemaName
FROM sys.all_objects ao
LEFT JOIN sys.schemas s
  ON s.schema_id = ao.schema_id
WHERE ao.[name] = @objectName