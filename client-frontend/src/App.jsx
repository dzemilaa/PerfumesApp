import { ApolloProvider } from '@apollo/client';
import { authClient } from './apolloClient';
import Header from './components/Header';
import Home from './Home';
import Cart from './components/Cart';
import Verify from './components/Verify';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Footer from './components/Footer';
import Favorite from './components/Favorite';
import Account from './components/Account';
import Orders from './components/Orders';
import Order from './components/Order';
import ProductsPage from './components/ProductPage';
import Products from './components/Products';
import ScrollToTop from './components/ScrollToTop';

function App() {
  return (
    <ApolloProvider client={authClient}>
      <BrowserRouter>
        <ScrollToTop />
        <Header />

        <Routes>
          <Route path='/order' element={<Order />} />
          <Route path='/products' element={<Products />} />
          <Route path="/" element={<Home />} />
          <Route path="/cart" element={<Cart />} />
          <Route path="/favorite" element={<Favorite />} />
          <Route path="/verify" element={<Verify />} />
          <Route path="/account" element={<Account />} />
          <Route path="/orders" element={<Orders />} />
          <Route path="/page/:id" element={<ProductsPage />} />
        </Routes>
        <Footer />
      </BrowserRouter>
    </ApolloProvider>
  );
}

export default App;
