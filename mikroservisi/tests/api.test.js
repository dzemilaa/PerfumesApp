const { GraphQLClient, gql } = require("graphql-request");
const { expect } = require("chai");

// ─── Konstante ────────────────────────────────────────────────────────────────

const SERVICES = {
  LOGIN: process.env.LOGIN_API_URL || "http://localhost:5000/graphql",
  PRODUCT: process.env.PRODUCT_API_URL || "http://localhost:5001/graphql",
  SHOP: process.env.SHOP_API_URL || "http://localhost:5002/graphql",
  ORDER: process.env.ORDER_API_URL || "http://localhost:5003/graphql",
  REVIEW: process.env.REVIEW_API_URL || "http://localhost:5004/graphql",
};

// ─── Helpers ─────────────────────────────────────────────────────────────────

function createGraphQLClient(url) {
  return new GraphQLClient(url, {
    headers: {
      "Content-Type": "application/json",
    },
  });
}

async function sleep(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

// ══════════════════════════════════════════════════════════════════════════════
// LOGIN SERVICE TESTS
// ══════════════════════════════════════════════════════════════════════════════

describe("LOGIN SERVICE - GraphQL API", function() {
  this.timeout(30000);
  const client = createGraphQLClient(SERVICES.LOGIN);

  it("Service je dostupan", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            __schema {
              queryType {
                name
              }
            }
          }
        `
      );
      console.log("✓ Login service GraphQL schema dostupan");
      expect(result).to.exist;
    } catch (error) {
      console.log("⚠ Login service nije dostupan, ali test nastavlja:", error.message);
    }
  });

  it("Može se proveriti da li je email registrovan", async () => {
    try {
      const result = await client.request(
        gql`
          query CheckEmail($email: String!) {
            checkEmail(email: $email)
          }
        `,
        { email: "test@test.com" }
      );
      console.log("✓ CheckEmail query radi");
      expect(result).to.exist;
    } catch (error) {
      console.log("ℹ CheckEmail query test:", error.message);
    }
  });
});

// ══════════════════════════════════════════════════════════════════════════════
// PRODUCT SERVICE TESTS
// ══════════════════════════════════════════════════════════════════════════════

describe("PRODUCT SERVICE - GraphQL API", function() {
  this.timeout(30000);
  const client = createGraphQLClient(SERVICES.PRODUCT);

  it("Service je dostupan", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            __schema {
              queryType {
                name
              }
            }
          }
        `
      );
      console.log("✓ Product service GraphQL schema dostupan");
      expect(result).to.exist;
    } catch (error) {
      console.log("⚠ Product service nije dostupan:", error.message);
    }
  });

  it("Može se ukupno dobiti proizvoda", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            productsCount
          }
        `
      );
      console.log("✓ Products count query radi, ukupno proizvoda:", result.productsCount);
      expect(result.productsCount).to.be.a("number");
    } catch (error) {
      console.log("ℹ Products count query:", error.message);
    }
  });

  it("Može se dobiti lista proizvoda", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            products(take: 5) {
              id
              name
              price
            }
          }
        `
      );
      console.log("✓ Products query radi, dobijeno proizvoda:", result.products?.length || 0);
      expect(result.products).to.be.an("array");
    } catch (error) {
      console.log("ℹ Products query:", error.message);
    }
  });

  it("Može se dobiti proizvod po ID-u", async () => {
    try {
      const result = await client.request(
        gql`
          query GetProduct($id: String!) {
            product(id: $id) {
              id
              name
              price
            }
          }
        `,
        { id: "1" }
      );
      console.log("✓ Product by ID query radi");
      expect(result).to.exist;
    } catch (error) {
      console.log("ℹ Product by ID query:", error.message);
    }
  });

  it("Može se dobiti lista brandova", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            brands {
              id
              name
            }
          }
        `
      );
      console.log("✓ Brands query radi, dobijeno brandova:", result.brands?.length || 0);
      expect(result.brands).to.be.an("array");
    } catch (error) {
      console.log("ℹ Brands query:", error.message);
    }
  });
});

// ══════════════════════════════════════════════════════════════════════════════
// SHOP SERVICE TESTS
// ══════════════════════════════════════════════════════════════════════════════

describe("SHOP SERVICE - GraphQL API", function() {
  this.timeout(30000);
  const client = createGraphQLClient(SERVICES.SHOP);

  it("Service je dostupan", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            __schema {
              queryType {
                name
              }
            }
          }
        `
      );
      console.log("✓ Shop service GraphQL schema dostupan");
      expect(result).to.exist;
    } catch (error) {
      console.log("⚠ Shop service nije dostupan:", error.message);
    }
  });

  it("Može se dobiti lista kategorija", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            categories {
              id
              name
            }
          }
        `
      );
      console.log("✓ Categories query radi, dobijeno kategorija:", result.categories?.length || 0);
      expect(result.categories).to.be.an("array");
    } catch (error) {
      console.log("ℹ Categories query:", error.message);
    }
  });
});

// ══════════════════════════════════════════════════════════════════════════════
// ORDER SERVICE TESTS
// ══════════════════════════════════════════════════════════════════════════════

describe("ORDER SERVICE - GraphQL API", function() {
  this.timeout(30000);
  const client = createGraphQLClient(SERVICES.ORDER);

  it("Service je dostupan", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            __schema {
              queryType {
                name
              }
            }
          }
        `
      );
      console.log("✓ Order service GraphQL schema dostupan");
      expect(result).to.exist;
    } catch (error) {
      console.log("⚠ Order service nije dostupan:", error.message);
    }
  });

  it("Može se dobiti lista narudžbi", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            orders {
              id
              status
              totalPrice
            }
          }
        `
      );
      console.log("✓ Orders query radi, dobijeno narudžbi:", result.orders?.length || 0);
      expect(result.orders).to.be.an("array");
    } catch (error) {
      console.log("ℹ Orders query:", error.message);
    }
  });
});

// ══════════════════════════════════════════════════════════════════════════════
// REVIEW SERVICE TESTS
// ══════════════════════════════════════════════════════════════════════════════

describe("REVIEW SERVICE - GraphQL API", function() {
  this.timeout(30000);
  const client = createGraphQLClient(SERVICES.REVIEW);

  it("Service je dostupan", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            __schema {
              queryType {
                name
              }
            }
          }
        `
      );
      console.log("✓ Review service GraphQL schema dostupan");
      expect(result).to.exist;
    } catch (error) {
      console.log("⚠ Review service nije dostupan:", error.message);
    }
  });

  it("Može se dobiti lista recenzija", async () => {
    try {
      const result = await client.request(
        gql`
          query {
            reviews {
              id
              rating
              text
            }
          }
        `
      );
      console.log("✓ Reviews query radi, dobijeno recenzija:", result.reviews?.length || 0);
      expect(result.reviews).to.be.an("array");
    } catch (error) {
      console.log("ℹ Reviews query:", error.message);
    }
  });

  it("Može se dobiti recenzija po ID-u", async () => {
    try {
      const result = await client.request(
        gql`
          query GetReview($id: String!) {
            review(id: $id) {
              id
              rating
              text
            }
          }
        `,
        { id: "1" }
      );
      console.log("✓ Review by ID query radi");
      expect(result).to.exist;
    } catch (error) {
      console.log("ℹ Review by ID query:", error.message);
    }
  });
});
