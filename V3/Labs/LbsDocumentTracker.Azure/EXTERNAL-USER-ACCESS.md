# Giving an external user access to Azure resources

This Portal walkthrough uses the existing `Training` resource group as an example.
It is a guide, not evidence that an invitation or role assignment has been made.
Obtain approval before granting access to shared training resources.

## 1. Understand the two steps

```text
External person's existing account
    |
    v
Invite into your Microsoft Entra workforce tenant as a Guest
    |
    v
Assign an Azure RBAC role at the required scope
    |
    v
Guest signs into your directory and accesses permitted resources
```

An invitation establishes a guest identity in your directory. It does not grant
access to your subscription or resource group.

Azure resource roles and Microsoft Entra directory roles are separate. Being
Owner of a resource group does not automatically make you an Entra user
administrator, and being an Entra administrator does not automatically grant
resource-group access.

## 2. Prerequisites

- Identify the directory associated with the target Azure subscription.
- Confirm the external person's email address through an approved channel.
- Agree what they need to do, the narrowest scope and when access should end.
- For invitations, use an account allowed by the tenant's guest invitation
  policy, such as a Guest Inviter or appropriately authorized administrator.
- For Azure role assignment, the operator needs
  `Microsoft.Authorization/roleAssignments/write` at the target scope, subject
  to any conditions. Examples include Owner, User Access Administrator and
  Role Based Access Control Administrator.
- Contributor alone cannot assign Azure roles.
- Tenant external-collaboration settings, cross-tenant access policies,
  Conditional Access or Azure policies may restrict the process.

Do not grant Global Administrator or subscription Owner just to complete a
routine guest invitation or resource-group assignment.

## 3. Invite an external person into Microsoft Entra ID

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com).
2. Switch to the directory that contains the Azure subscription.
3. Open **Entra ID > Users > All users**.
4. Select **New user > Invite external user**.
5. Enter the person's email address and display name.
6. Review the invitation and select **Invite**.
7. Ask the person to open the invitation and accept it with the intended account.
8. Reopen their user record and check invitation acceptance/redemption status.

Portal labels may vary slightly. The person's identity provider or tenant
configuration determines the sign-in experience; it may use a work/school
account, Microsoft account or email one-time passcode.

You normally do not create or share a password for a guest. Do not ask them for
their password or MFA codes.

### Guest invitation versus creating a new member account

| Situation | Normal approach |
|---|---|
| Collaborator outside your organization who already has an account | Invite external user |
| Employee needing an account managed by your organization | Create new user, following your organization's process |

Use the tenant's approved account lifecycle policy. Do not convert a guest into
a member or weaken cross-tenant restrictions merely to work around an error.

## 4. Select a role and scope

Start with the least privilege needed.

| Requirement | Typical Azure role | Caution |
|---|---|---|
| View resources and configuration | Reader | Does not normally grant application data access |
| Create, modify and delete resources | Contributor | Can disrupt services, deploy code and incur costs |
| Manage resources and delegate access | Owner | Broad privilege; avoid for ordinary attendees |

Contributor's inability to assign roles does not make it harmless: resource
management can still expose sensitive configuration or affect workloads.

Scope determines where the assignment applies:

```text
Subscription
    -> Resource group: Training
        -> Individual resource
            -> Supported child-resource scope, such as a Service Bus queue
```

An assignment on `Training` applies to its resources through inheritance,
including future resources placed there. If only one resource is required,
prefer that resource's scope. Avoid subscription-wide access for a single lab.

## 5. Assign Reader on the Training resource group

1. Sign in to the [Azure Portal](https://portal.azure.com).
2. Confirm the correct directory and subscription.
3. Open **Resource groups > Training**.
4. Open **Access control (IAM)**.
5. Select **Add > Add role assignment**.
6. Select **Reader**, then **Next**.
7. Under Members, select **User, group, or service principal**.
8. Choose **Select members** and locate the invited guest.
9. Verify their email and identity; do not rely on display name alone.
10. Select **Review + assign** and complete the assignment.
11. Confirm the assignment appears at the intended scope.

If Contributor is approved instead, use the same flow with Contributor. Do not
grant Owner as a shortcut when the correct role is missing or unavailable.

For a group of attendees, consider a dedicated Entra security group with the
approved role at the approved scope, then manage membership. Review all roles
already assigned to the group before adding guests. Use time-bound access
through approved governance/PIM capabilities where available and licensed;
otherwise record an expiry date and perform manual removal.

## 6. Guest sign-in and verification

The guest should:

1. Accept the invitation.
2. Sign in to Azure Portal with the invited account.
3. Open **Directories + subscriptions** and switch to your directory.
4. Confirm the target subscription is included in their Portal filters.
5. Open **Resource groups > Training**, or follow a resource link supplied by
   an authorized operator.

Allow several minutes for role propagation; refresh or sign out/in if necessary.
Directory switch and role assignment are both important: accepting an invitation
does not automatically switch the guest's Portal to your directory.

### Acceptance checklist

- [ ] The user is present as a guest in the correct directory.
- [ ] The invitation was accepted using the intended account.
- [ ] The assignment targets the correct guest or approved group.
- [ ] The role is scoped to Training or a narrower approved resource.
- [ ] The guest can view the expected resources.
- [ ] A Reader cannot perform unapproved management operations.
- [ ] Unrelated resource groups are not accessible unless separately authorized.
- [ ] The access expiry/review date and owner are recorded.

Verify permissions without making destructive test changes to shared resources.

## 7. Management access versus data access in this project

| Task | Required access | Is RG Reader alone enough? |
|---|---|---|
| View SQL server, Function App and queue resource properties | Reader on the relevant resource/scope | Generally yes for management properties |
| Modify Function deployment or resource settings | Appropriate management role and deployment permissions | No |
| Send Service Bus messages | Azure Service Bus Data Sender on the queue | No |
| Peek/receive Service Bus messages | Azure Service Bus Data Receiver on the queue | No |
| Replay approved dead letters | Receiver and Sender plus an approved recovery procedure | No |
| Read SQL application tables | Contained Entra database user/group, SELECT grants and network access | No |
| Administer directory users | Appropriate Entra directory permission | No |
| Use the future hosted website | Website authentication and application authorization | No RG role is required just to use the application |

Resource-level permissions, inherited permissions and existing group membership
can change effective access. Review them instead of assuming a single assignment
is the user's only permission.

### Service Bus example

For an approved messaging exercise:

1. Open `fristAzureServiceBus > Queues > document-processing`.
2. Open **Access control (IAM)**.
3. Assign the guest or approved group the required data role.
4. Use **Data Receiver** for authorized inspection/receiving and **Data Sender**
   for sending; assign both only when the exercise requires both.
5. Allow propagation and verify access.

Service Bus Explorer can consume, delete or resend messages depending on the
operation. Use **Peek** for investigation, not Receive-and-delete. Do not grant
Data Owner simply to avoid identifying the required role.

### SQL example

The SQL user `fristAzureFunctionApp` represents the Function's managed identity.
It is not the external person's account. Do not share that identity or grant a
person access by pretending to be the Function.

If a guest genuinely needs direct SQL access, an authorized Entra SQL
administrator must resolve the guest (or an approved Entra group) and create a
contained user in **firstdb**. The account must exist in the SQL server's tenant.
Guest identity naming/resolution can differ from the invitation email; verify
the intended principal before creating it.

Example pattern only, using the administrator-resolved identity name:

```sql
-- Connect to firstdb as an authorized Microsoft Entra administrator.
-- Replace the placeholder with the verified external principal name.
CREATE USER [verified-external-principal] FROM EXTERNAL PROVIDER;

GRANT SELECT ON OBJECT::lbs_training.Documents TO [verified-external-principal];
```

Do not run the placeholder literally or rerun CREATE USER when the user already
exists. Directory resolution may need additional authorized directory access.
Prefer approved groups for repeatable access management.

SELECT on Documents exposes all accessible rows and columns, including queued
document text. For real data, use an approved restricted view or appropriate row/
column controls instead of copying this training example. Do not grant Outbox
access or write permissions unless required by the exercise.

Direct SQL access also requires:

- Microsoft Entra authentication using the correct tenant/account.
- SQL network access for the caller's connection.
- Only the necessary SQL permissions; not db_owner.

The SQL **Allow Azure services and resources** exception permits Azure-origin
connections. It does not automatically allow the guest's home/office laptop.
Their approved public IP allowance or another approved network path may still
be required. Managed identity and guest identity do not bypass the firewall.

See [Architecture and operations](ARCHITECTURE-AND-OPERATIONS.md) for the
Function's existing SQL user, network trade-offs and full permission matrix.

## 8. Troubleshooting

| Problem | Checks |
|---|---|
| Invite option is unavailable | Correct tenant, invitation permission and external-collaboration policy |
| Invitation cannot be redeemed | Correct invited account, invitation status and cross-tenant/Conditional Access restrictions |
| Guest not found in role member picker | Guest exists in this tenant, correct email/object ID, directory propagation |
| Add role assignment is disabled | Operator has roleAssignments/write at the scope; activate eligible role if applicable |
| Guest sees no resources | Correct directory, accepted invitation, role scope, Portal subscription filters and propagation |
| Reader sees resource but cannot read SQL | Expected separation: configure approved SQL user/grants and network access |
| Contributor cannot receive queue messages | Assign the required Service Bus data role; management role is not a data role |
| SQL rejects a client IP | Network/firewall issue; adding SQL GRANTs will not fix it |
| Access persists after one assignment is removed | Check inherited roles, group membership, other assignments and token/cache propagation |

Do not disable MFA, grant Global Administrator/Owner or share credentials to
resolve these problems. Use sign-in logs and access checks through an authorized
administrator where necessary.

## 9. Review and remove access

At the end of training:

1. Remove the approved Azure role assignments, or remove membership of the
   dedicated access group as appropriate.
2. Review inherited access and any separate Service Bus data-role assignments.
3. Revoke/remove direct SQL permissions or user mappings through an authorized
   SQL administrator if they were created for the exercise.
4. Remove application access and other exercise-specific grants.
5. Check whether the guest is used elsewhere before blocking or deleting their
   directory account. Coordinate tenant-wide changes with the identity owner.
6. Allow for propagation and verify the intended access is no longer available.

Removing an RG role alone is not a complete offboarding process: SQL grants,
application authorization and other resource assignments are separate.

Never delete shared resources as part of removing a user's access.

## 10. References

- [Invite a guest user in Microsoft Entra ID](https://learn.microsoft.com/en-us/entra/external-id/b2b-quickstart-add-guest-users-portal)
- [Assign Azure roles using the Portal](https://learn.microsoft.com/en-us/azure/role-based-access-control/role-assignments-portal)
- [Azure RBAC scope](https://learn.microsoft.com/en-us/azure/role-based-access-control/scope-overview)
- [Service Bus managed identity and data roles](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-managed-service-identity)
- [Azure SQL Microsoft Entra authentication](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-overview)
