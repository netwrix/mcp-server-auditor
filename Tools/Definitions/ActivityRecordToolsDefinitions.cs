namespace NetwrixAuditorMCPServer.Tools.Definitions;

public static class ActivityRecordToolsDefinitions
{
    public const string FilterListDescription = @"<FilterListDescription>
  <Filters>
    <Filter>
      <Name>RID</Name>
    <Description>Activity Record ID (max length: 49)</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Equals"": ""some-rid""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Who</Name>
    <Description>User who made the change (e.g., Enterprise\ Administrator, administrator@enterprise.onmicros oft.com)</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWi InGroup, NotInGroup</SupportedOperators>
    <Examples>
      <Example>{""Contains"": ""Admin""}</Example>
      <Example>{""NotInGroup"": ""Guests""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Where</Name>
    <Description>Resource where the change was made. The resource name can be a FQDN or NETBIOS server name, Active Directory domain or container, SQL Server instance, SharePoint farm, VMware host, etc.</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""StartsWith"": ""Server-""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>ObjectType</Name>
    <Description>Type of objects</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Equals"": ""User""}</Example>
      <Example>{""NotEqualTo"": ""Group""}</Example>
      <Example>{""Equals"": ""Stored Procedure""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>What</Name>
    <Description>Specific object changed in the audit activity event (policy name, file name, user name, OU name etc.)</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWi EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Contains"": ""Policy""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>DataSource</Name>
    <Description>Data source for the activity</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <PredefinedValues>
      <PredefinedValue>AD FS</PredefinedValue>
      <PredefinedValue>Active Directory</PredefinedValue>
      <PredefinedValue>Entra ID</PredefinedValue>
      <PredefinedValue>Event Log</PredefinedValue>
      <PredefinedValue>Exchange</PredefinedValue>
      <PredefinedValue>Exchange Online</PredefinedValue>
      <PredefinedValue>File Servers</PredefinedValue>
      <PredefinedValue>Group Policy</PredefinedValue>
      <PredefinedValue>Inactive Users</PredefinedValue>
      <PredefinedValue>Logon Activity</PredefinedValue>
      <PredefinedValue>Network Device</PredefinedValue>
      <PredefinedValue>Oracle Database</PredefinedValue>
      <PredefinedValue>Password Expiration</PredefinedValue>
      <PredefinedValue>SharePoint</PredefinedValue>
      <PredefinedValue>SharePoint Online</PredefinedValue>
      <PredefinedValue>SQL Server</PredefinedValue>
      <PredefinedValue>User Activity</PredefinedValue>
      <PredefinedValue>VMware</PredefinedValue>
      <PredefinedValue>Windows Server</PredefinedValue>
      <PredefinedValue>Entitlement reviews</PredefinedValue>
      <PredefinedValue>MS Teams</PredefinedValue>
    </PredefinedValues>
    <Examples>
      <Example>{""Equals"": ""Active Directory""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>MonitoringPlan</Name>
    <Description>Specific monitoring plan - Netwrix Auditor object that governs data collection</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Equals"": ""Default Plan""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Item</Name>
    <Description>Limits your search to a specific item —object of monitoring—and its type provided in brackets</Description>
    <SupportedOperators>(default), Equals, NotEqual StartsWith, EndsWith</SupportedOperators>
    <PredefinedValues>
      <PredefinedValue>AD container</PredefinedValue>
        <PredefinedValue>NetApp</PredefinedValue>
        <PredefinedValue>Computer</PredefinedValue>
        <PredefinedValue>Office 365 tenant</PredefinedValue>
        <PredefinedValue>Domain</PredefinedValue>
        <PredefinedValue>Oracle Database instance</PredefinedValue>
        <PredefinedValue>EMC Isilon</PredefinedValue>
        <PredefinedValue>SharePoint farm</PredefinedValue>
        <PredefinedValue>EMC VNX/VNXe</PredefinedValue>
        <PredefinedValue>SQL Server instance</PredefinedValue>
        <PredefinedValue>Integration</PredefinedValue>
        <PredefinedValue>VMware ESX/ESXi/vCenter</PredefinedValue>
        <PredefinedValue>IP range</PredefinedValue>
        <PredefinedValue>Windows file share</PredefinedValue>
    </PredefinedValues>
    <Examples>
      <Example>{""Contains"": ""Domain""</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Workstation</Name>
    <Description>Originating workstation from which the change was made (e.g., WKSwin12.enterprise.local)</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""EndsWith"": "".local""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Detail</Name>
    <Description>Limits your search results to entries that contain the specified information in Detail. Normally contains information specific to your data source, e.g., assigned permissions, before and after values, start and end dates. This filter can be helpful when you are looking for a unique entry.</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Contains"": ""permission""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Before</Name>
    <Description>Limits your search results to entries that contain the specified before value in Detail.</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Contains"": ""100""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>After</Name>
    <Description>Limits your search results to entries that contain the specified after value in the Detail.</Description>
    <SupportedOperators>Contains (default), Equals, NotEqualTo, StartsWith, EndsWith</SupportedOperators>
    <Examples>
      <Example>{""Contains"": ""200""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>Action</Name>
    <Description>The type of action performed in the audit record</Description>
    <SupportedOperators>Equals (default), NotEqualTo</SupportedOperators>
    <PredefinedValues>
      <PredefinedValue>Added</PredefinedValue>
      <PredefinedValue>Add (Failed Attempt)</PredefinedValue>
      <PredefinedValue>Removed</PredefinedValue>
      <PredefinedValue>Remove (Failed Attempt)</PredefinedValue>
      <PredefinedValue>Modified</PredefinedValue>
      <PredefinedValue>Modify (Failed Attempt)</PredefinedValue>
      <PredefinedValue>Read</PredefinedValue>
      <PredefinedValue>Read (Failed Attempt)</PredefinedValue>
      <PredefinedValue>Moved</PredefinedValue>
      <PredefinedValue>Move (Failed Attempt)</PredefinedValue>
      <PredefinedValue>Renamed</PredefinedValue>
      <PredefinedValue>Rename (Failed Attempt)</PredefinedValue>
      <PredefinedValue>Checked in</PredefinedValue>
      <PredefinedValue>Checked out</PredefinedValue>
      <PredefinedValue>Discard check out</PredefinedValue>
      <PredefinedValue>Successful Logon</PredefinedValue>
      <PredefinedValue>Failed Logon</PredefinedValue>
      <PredefinedValue>Logoff</PredefinedValue>
      <PredefinedValue>Copied</PredefinedValue>
      <PredefinedValue>Sent</PredefinedValue>
      <PredefinedValue>Session start</PredefinedValue>
      <PredefinedValue>Session end</PredefinedValue>
      <PredefinedValue>Activated</PredefinedValue>
    </PredefinedValues>
    <Examples>
      <Example>{""Equals"": ""Modified""}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>When</Name>
    <Description>Time range filter. Netwrix Auditor supports the following for the When filter: Use Equals (default operator) or NotEqualTo operator to specify time interval. Use Within timeframe with one of the enumerated values (Today, Yesterday, etc.), and/or values in the To and From. To and From support the following date time formats: YYYY-mm-ddTHH:MM:SSZ — Indicates UTC time (zero offset, preffered option); YYYY-mmddTHH:MM:SS+HH:MM — Indicates time zones ahead of UTC (positive offset); YYYY-mm-ddTHH:MM:SSHH:MM—Indicates time zones behind UTC (negative offset)</Description>
    <SupportedOperators>""From..To"" interval, Equals (default), NotEqualTo</SupportedOperators>
    <Examples>
      <Example>{""From"":""2025-04-18T09:16:33Z"",""To"":""2025-04-21T09:16:35Z""}</Example>
      <Example>{""LastSevenDays"": """"}</Example>
    </Examples>
    </Filter>
    <Filter>
      <Name>WorkingHours</Name>
    <Description>Limits your search to the specified working hours. You can track activity outside the business hours applying the NotEqualTo operator. To and From support the same formats as the `When` filter</Description>
    <SupportedOperators>""From..To"" interval, Equals (default), NotEqualTo</SupportedOperators>
    <Examples>
      <Example>{""From"": ""09:00:00+00:00"", ""To"": ""17:00:00+00:00""}</Example>
    </Examples>
    </Filter>
  </Filters>
  <Examples>
    <Example>
      <FilterList>{""FilterList"":{""Action"":[{""Equals"":""Modified""}],""Who"":[""Administrator""}]}</FilterList>
    <ExpectedResult>Finds modifications by Administrator</ExpectedResult>
    </Example>
    <Example>
      <FilterList>{""What"":[{""Contains"":""password""}],""When"":[{""LastSevenDays"":""""}]}}</FilterList>
    <ExpectedResult>Finds password-related changes in the last 7 days</ExpectedResult>
    </Example>
    <Example>
      <FilterList>{""Where"":[{""Contains"":""SQL""}],""Who"":[{""NotEqualTo"":""Administrator""}]}}</FilterList>
    <ExpectedResult>Finds changes on SQL servers by non-administrator</ExpectedResult>
    </Example>
  </Examples>
</FilterListDescription>";
}