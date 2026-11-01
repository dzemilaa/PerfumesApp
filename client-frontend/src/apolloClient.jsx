import { ApolloClient, InMemoryCache, createHttpLink } from "@apollo/client";
import fetch from "cross-fetch";

const serviceUrl = (environmentUrl, localPort, productionUrl) =>
  environmentUrl ||
  (import.meta.env.DEV ? `http://localhost:${localPort}/graphql` : productionUrl);

const authClient = new ApolloClient({
  link: createHttpLink({
    uri: serviceUrl(
      import.meta.env.VITE_LOGIN_API_URL,
      5000,
      "https://loginservice-production-ab1f.up.railway.app/graphql",
    ),
    credentials: "include",//cookies i sessions
    fetch,
  }),
  cache: new InMemoryCache(),
});

const productsClient = new ApolloClient({
  link: createHttpLink({
    uri: serviceUrl(
      import.meta.env.VITE_PRODUCT_API_URL,
      5001,
      "https://productservice-production-c2fd.up.railway.app/graphql",
    ),
  }),
  cache: new InMemoryCache(),
});

const shopClient = new ApolloClient({
  link: createHttpLink({
    uri: serviceUrl(
      import.meta.env.VITE_SHOP_API_URL,
      5002,
      "https://shopservice-production.up.railway.app/graphql",
    ),
  }),
  cache: new InMemoryCache(),
});

const orderClient = new ApolloClient({
  link: createHttpLink({
    uri: serviceUrl(
      import.meta.env.VITE_ORDER_API_URL,
      5003,
      "https://orderservice-production-4f8d.up.railway.app/graphql",
    ),
    credentials: "include",
    fetch,

  }),
  cache: new InMemoryCache(),
});

const reviewClient = new ApolloClient({
  link: createHttpLink({
    uri: serviceUrl(
      import.meta.env.VITE_REVIEW_API_URL,
      5004,
      "https://reviewservice-production.up.railway.app/graphql",
    ),
  }),
  cache: new InMemoryCache(),
});
export { authClient, productsClient, shopClient, orderClient, reviewClient };
