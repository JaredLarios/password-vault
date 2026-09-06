# Backend User Story
- Story ID: US-XXX
- Title: Short description of the user story

## User Story
As a [type of user], I want to [action], so that [benefit/value].

### Business Requirement
Describe what the system needs to accomplish from a business perspective.

## API Specification
### Endpoint
- HTTP Method: POST
- Route: /api/products

### Authentication
- [ ] Public
- [ ] JWT required

### Request
- POST /api/products
- Authorization: Cookie <token> 
- Content-Type: application/json

### Request Body
- [ ] Body
- [ ] Param
```json
{
    "name": "Product name"
}
```

### Request Fields
| Field | Type | Required | Validation | Description |
| --- | --- | --- | --- | --- |
| name | string | Yes | Max 100 chars | Product name |
| --- | --- | --- | --- | --- |

## Response Specification
### Success Response
- HTTP Status: 201 Created
```json
{
    "id": 1,
    "name": "Product name"
}
```

### Error Responses
#### 400 – Bad Request
```json
{
  "message": "Invalid product data."
}
```

#### 401 – Unauthorized
```json
{
  "message": "Authentication required."
}
```

#### 500 – Internal Server Error
```json
{
  "message": "An unexpected error occurred."
}
```
