// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Base.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace KdSoft.Services.Protobuf {

  /// <summary>Holder for reflection information generated from Base.proto</summary>
  public static partial class BaseReflection {

    #region Descriptor
    /// <summary>File descriptor for Base.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BaseReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgpCYXNlLnByb3RvEgRCYXNlIi0KCE9wU3RhdHVzEgwKBENvZGUYASABKAUS",
            "EwoLRGVzY3JpcHRpb24YAiABKAkiBwoFRW1wdHkiSQoEVGltZRIMCgRIb3Vy",
            "GAEgASgFEg4KBk1pbnV0ZRgCIAEoBRIOCgZTZWNvbmQYAyABKAUSEwoLTWls",
            "bGlTZWNvbmQYBCABKAUiMAoERGF0ZRIMCgRZZWFyGAEgASgFEg0KBU1vbnRo",
            "GAIgASgFEgsKA0RheRgDIAEoBSI+CghEYXRlVGltZRIYCgREYXRlGAEgASgL",
            "MgouQmFzZS5EYXRlEhgKBFRpbWUYAiABKAsyCi5CYXNlLlRpbWUiLQoMU2Vy",
            "dmljZUVycm9yEgwKBENvZGUYASABKAUSDwoHTWVzc2FnZRgCIAEoCUIbqgIY",
            "S2RTb2Z0LlNlcnZpY2VzLlByb3RvYnVmYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::KdSoft.Services.Protobuf.OpStatus), global::KdSoft.Services.Protobuf.OpStatus.Parser, new[]{ "Code", "Description" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::KdSoft.Services.Protobuf.Empty), global::KdSoft.Services.Protobuf.Empty.Parser, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::KdSoft.Services.Protobuf.Time), global::KdSoft.Services.Protobuf.Time.Parser, new[]{ "Hour", "Minute", "Second", "MilliSecond" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::KdSoft.Services.Protobuf.Date), global::KdSoft.Services.Protobuf.Date.Parser, new[]{ "Year", "Month", "Day" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::KdSoft.Services.Protobuf.DateTime), global::KdSoft.Services.Protobuf.DateTime.Parser, new[]{ "Date", "Time" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::KdSoft.Services.Protobuf.ServiceError), global::KdSoft.Services.Protobuf.ServiceError.Parser, new[]{ "Code", "Message" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class OpStatus : pb::IMessage<OpStatus> {
    private static readonly pb::MessageParser<OpStatus> _parser = new pb::MessageParser<OpStatus>(() => new OpStatus());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<OpStatus> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::KdSoft.Services.Protobuf.BaseReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpStatus() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpStatus(OpStatus other) : this() {
      code_ = other.code_;
      description_ = other.description_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpStatus Clone() {
      return new OpStatus(this);
    }

    /// <summary>Field number for the "Code" field.</summary>
    public const int CodeFieldNumber = 1;
    private int code_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Code {
      get { return code_; }
      set {
        code_ = value;
      }
    }

    /// <summary>Field number for the "Description" field.</summary>
    public const int DescriptionFieldNumber = 2;
    private string description_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Description {
      get { return description_; }
      set {
        description_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as OpStatus);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(OpStatus other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Code != other.Code) return false;
      if (Description != other.Description) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Code != 0) hash ^= Code.GetHashCode();
      if (Description.Length != 0) hash ^= Description.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Code != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Code);
      }
      if (Description.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Description);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Code != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Code);
      }
      if (Description.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Description);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(OpStatus other) {
      if (other == null) {
        return;
      }
      if (other.Code != 0) {
        Code = other.Code;
      }
      if (other.Description.Length != 0) {
        Description = other.Description;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Code = input.ReadInt32();
            break;
          }
          case 18: {
            Description = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Empty : pb::IMessage<Empty> {
    private static readonly pb::MessageParser<Empty> _parser = new pb::MessageParser<Empty>(() => new Empty());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Empty> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::KdSoft.Services.Protobuf.BaseReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Empty() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Empty(Empty other) : this() {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Empty Clone() {
      return new Empty(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Empty);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Empty other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Empty other) {
      if (other == null) {
        return;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
        }
      }
    }

  }

  public sealed partial class Time : pb::IMessage<Time> {
    private static readonly pb::MessageParser<Time> _parser = new pb::MessageParser<Time>(() => new Time());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Time> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::KdSoft.Services.Protobuf.BaseReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Time() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Time(Time other) : this() {
      hour_ = other.hour_;
      minute_ = other.minute_;
      second_ = other.second_;
      milliSecond_ = other.milliSecond_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Time Clone() {
      return new Time(this);
    }

    /// <summary>Field number for the "Hour" field.</summary>
    public const int HourFieldNumber = 1;
    private int hour_;
    /// <summary>
    /// 0 - 23
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Hour {
      get { return hour_; }
      set {
        hour_ = value;
      }
    }

    /// <summary>Field number for the "Minute" field.</summary>
    public const int MinuteFieldNumber = 2;
    private int minute_;
    /// <summary>
    /// 0 - 59
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Minute {
      get { return minute_; }
      set {
        minute_ = value;
      }
    }

    /// <summary>Field number for the "Second" field.</summary>
    public const int SecondFieldNumber = 3;
    private int second_;
    /// <summary>
    /// 0 - 59
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Second {
      get { return second_; }
      set {
        second_ = value;
      }
    }

    /// <summary>Field number for the "MilliSecond" field.</summary>
    public const int MilliSecondFieldNumber = 4;
    private int milliSecond_;
    /// <summary>
    /// 0 - 999
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int MilliSecond {
      get { return milliSecond_; }
      set {
        milliSecond_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Time);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Time other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Hour != other.Hour) return false;
      if (Minute != other.Minute) return false;
      if (Second != other.Second) return false;
      if (MilliSecond != other.MilliSecond) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Hour != 0) hash ^= Hour.GetHashCode();
      if (Minute != 0) hash ^= Minute.GetHashCode();
      if (Second != 0) hash ^= Second.GetHashCode();
      if (MilliSecond != 0) hash ^= MilliSecond.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Hour != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Hour);
      }
      if (Minute != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Minute);
      }
      if (Second != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Second);
      }
      if (MilliSecond != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(MilliSecond);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Hour != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Hour);
      }
      if (Minute != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Minute);
      }
      if (Second != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Second);
      }
      if (MilliSecond != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(MilliSecond);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Time other) {
      if (other == null) {
        return;
      }
      if (other.Hour != 0) {
        Hour = other.Hour;
      }
      if (other.Minute != 0) {
        Minute = other.Minute;
      }
      if (other.Second != 0) {
        Second = other.Second;
      }
      if (other.MilliSecond != 0) {
        MilliSecond = other.MilliSecond;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Hour = input.ReadInt32();
            break;
          }
          case 16: {
            Minute = input.ReadInt32();
            break;
          }
          case 24: {
            Second = input.ReadInt32();
            break;
          }
          case 32: {
            MilliSecond = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Date : pb::IMessage<Date> {
    private static readonly pb::MessageParser<Date> _parser = new pb::MessageParser<Date>(() => new Date());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Date> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::KdSoft.Services.Protobuf.BaseReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Date() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Date(Date other) : this() {
      year_ = other.year_;
      month_ = other.month_;
      day_ = other.day_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Date Clone() {
      return new Date(this);
    }

    /// <summary>Field number for the "Year" field.</summary>
    public const int YearFieldNumber = 1;
    private int year_;
    /// <summary>
    /// 1753 onwards
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Year {
      get { return year_; }
      set {
        year_ = value;
      }
    }

    /// <summary>Field number for the "Month" field.</summary>
    public const int MonthFieldNumber = 2;
    private int month_;
    /// <summary>
    /// 1 - 12
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Month {
      get { return month_; }
      set {
        month_ = value;
      }
    }

    /// <summary>Field number for the "Day" field.</summary>
    public const int DayFieldNumber = 3;
    private int day_;
    /// <summary>
    /// 1 - 31
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Day {
      get { return day_; }
      set {
        day_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Date);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Date other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Year != other.Year) return false;
      if (Month != other.Month) return false;
      if (Day != other.Day) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Year != 0) hash ^= Year.GetHashCode();
      if (Month != 0) hash ^= Month.GetHashCode();
      if (Day != 0) hash ^= Day.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Year != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Year);
      }
      if (Month != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Month);
      }
      if (Day != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Day);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Year != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Year);
      }
      if (Month != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Month);
      }
      if (Day != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Day);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Date other) {
      if (other == null) {
        return;
      }
      if (other.Year != 0) {
        Year = other.Year;
      }
      if (other.Month != 0) {
        Month = other.Month;
      }
      if (other.Day != 0) {
        Day = other.Day;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Year = input.ReadInt32();
            break;
          }
          case 16: {
            Month = input.ReadInt32();
            break;
          }
          case 24: {
            Day = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class DateTime : pb::IMessage<DateTime> {
    private static readonly pb::MessageParser<DateTime> _parser = new pb::MessageParser<DateTime>(() => new DateTime());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<DateTime> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::KdSoft.Services.Protobuf.BaseReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DateTime() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DateTime(DateTime other) : this() {
      Date = other.date_ != null ? other.Date.Clone() : null;
      Time = other.time_ != null ? other.Time.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DateTime Clone() {
      return new DateTime(this);
    }

    /// <summary>Field number for the "Date" field.</summary>
    public const int DateFieldNumber = 1;
    private global::KdSoft.Services.Protobuf.Date date_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::KdSoft.Services.Protobuf.Date Date {
      get { return date_; }
      set {
        date_ = value;
      }
    }

    /// <summary>Field number for the "Time" field.</summary>
    public const int TimeFieldNumber = 2;
    private global::KdSoft.Services.Protobuf.Time time_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::KdSoft.Services.Protobuf.Time Time {
      get { return time_; }
      set {
        time_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as DateTime);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(DateTime other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Date, other.Date)) return false;
      if (!object.Equals(Time, other.Time)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (date_ != null) hash ^= Date.GetHashCode();
      if (time_ != null) hash ^= Time.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (date_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Date);
      }
      if (time_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Time);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (date_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Date);
      }
      if (time_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Time);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(DateTime other) {
      if (other == null) {
        return;
      }
      if (other.date_ != null) {
        if (date_ == null) {
          date_ = new global::KdSoft.Services.Protobuf.Date();
        }
        Date.MergeFrom(other.Date);
      }
      if (other.time_ != null) {
        if (time_ == null) {
          time_ = new global::KdSoft.Services.Protobuf.Time();
        }
        Time.MergeFrom(other.Time);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (date_ == null) {
              date_ = new global::KdSoft.Services.Protobuf.Date();
            }
            input.ReadMessage(date_);
            break;
          }
          case 18: {
            if (time_ == null) {
              time_ = new global::KdSoft.Services.Protobuf.Time();
            }
            input.ReadMessage(time_);
            break;
          }
        }
      }
    }

  }

  public sealed partial class ServiceError : pb::IMessage<ServiceError> {
    private static readonly pb::MessageParser<ServiceError> _parser = new pb::MessageParser<ServiceError>(() => new ServiceError());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ServiceError> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::KdSoft.Services.Protobuf.BaseReflection.Descriptor.MessageTypes[5]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ServiceError() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ServiceError(ServiceError other) : this() {
      code_ = other.code_;
      message_ = other.message_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ServiceError Clone() {
      return new ServiceError(this);
    }

    /// <summary>Field number for the "Code" field.</summary>
    public const int CodeFieldNumber = 1;
    private int code_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Code {
      get { return code_; }
      set {
        code_ = value;
      }
    }

    /// <summary>Field number for the "Message" field.</summary>
    public const int MessageFieldNumber = 2;
    private string message_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ServiceError);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ServiceError other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Code != other.Code) return false;
      if (Message != other.Message) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Code != 0) hash ^= Code.GetHashCode();
      if (Message.Length != 0) hash ^= Message.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Code != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Code);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Message);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Code != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Code);
      }
      if (Message.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ServiceError other) {
      if (other == null) {
        return;
      }
      if (other.Code != 0) {
        Code = other.Code;
      }
      if (other.Message.Length != 0) {
        Message = other.Message;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Code = input.ReadInt32();
            break;
          }
          case 18: {
            Message = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
