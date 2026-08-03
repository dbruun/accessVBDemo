Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Namespace DataAccess

    Public Class EfSettingsRepository

        Private ReadOnly _context As ChompManDbContext

        Public Sub New(context As ChompManDbContext)
            _context = context
        End Sub

        Public Function GetAll() As List(Of SettingEntry)
            Return _context.Settings.AsNoTracking().
                OrderBy(Function(setting) setting.Key).
                Select(Function(setting) New SettingEntry With {
                    .Key = setting.Key,
                    .Value = setting.Value
                }).ToList()
        End Function

        Public Function GetValue(key As String, Optional defaultValue As String = "") As String
            Dim value = _context.Settings.AsNoTracking().
                Where(Function(setting) setting.Key = key).
                Select(Function(setting) setting.Value).
                SingleOrDefault()
            Return If(value Is Nothing, defaultValue, value)
        End Function

        Public Sub Upsert(key As String, value As String)
            Dim setting = _context.Settings.Find(key)
            If setting Is Nothing Then
                _context.Settings.Add(New SettingEntity With {.Key = key, .Value = value})
            Else
                setting.Value = value
            End If
            _context.SaveChanges()
        End Sub

    End Class

End Namespace
